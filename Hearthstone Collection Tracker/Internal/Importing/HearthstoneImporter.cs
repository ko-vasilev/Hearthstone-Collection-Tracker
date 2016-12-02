using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthMirror;
using HearthMirror.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class HearthstoneImporter
    {
        private const string LOGGER_CATEGORY = "CollectionTrackerHSImport";

        protected double WindowXRatioTo1920 { get; set; }

        protected double WindowYRatioTo1080 { get; set; }

        protected Point SearchBoxPosition { get; set; }

        protected IntPtr HearthstoneWindow { get; set; }

        public int ImportStepDelay { get; set; }

        public bool PasteFromClipboard { get; set; }

        public bool NonGoldenFirst { get; set; }

        public async Task<List<BasicSetCollectionInfo>> Import(string importingSet)
        {
            var sets = SetCardsManager.CreateEmptyCollection();
            if (!string.IsNullOrEmpty(importingSet))
            {
                sets = sets.Where(s => s.SetName == importingSet).ToList();
            }

            if (!sets.Any())
            {
                return sets;
            }

            try
            {
                HideHDTOverlay();

                var collection = Reflection.GetCollection();
                var goldenCollection = Reflection.GetCollection().Where(x => x.Premium == true);
                var commonCollection = Reflection.GetCollection().Where(x => x.Premium == false);

                foreach (var set in sets)
                {
                    foreach (var card in set.Cards)
                    {
                        var amountGolden = goldenCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();
                        var amountNonGolden = commonCollection.Where(x => x.Id.Equals(card.CardId)).Select(x=> x.Count).FirstOrDefault();
                        
                        card.AmountNonGolden = Math.Min(amountNonGolden, card.MaxAmountInCollection);
                        card.AmountGolden = Math.Min(amountGolden, card.MaxAmountInCollection);
                    }                    
                }                
            }
            catch (ImportingException)
            {
                ShowHDTOverlay();
                throw;
            }
            catch(Exception e)
            {
                ShowHDTOverlay();
                throw new ImportingException("Unexpected exception occured during importing", e);
            }
            finally
            {
                ShowHDTOverlay();
            }

            return sets;
        }

        private void HideHDTOverlay()
        {
            Hearthstone_Deck_Tracker.API.Core.OverlayWindow.ForceHidden = true;
            Hearthstone_Deck_Tracker.API.Core.OverlayWindow.UpdatePosition();
        }

        private void ShowHDTOverlay()
        {
            Hearthstone_Deck_Tracker.API.Core.OverlayWindow.ForceHidden = false;
            Hearthstone_Deck_Tracker.API.Core.OverlayWindow.UpdatePosition();
        }

        private static double GetXPos(double left, int width, double ratio)
        {
            return (width * ratio * left) + (width * (1 - ratio) / 2);
        }

        private static async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint, int delay = 0)
        {
            User32.ClientToScreen(wndHandle, ref clientPoint);

            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            //mouse down
            if (SystemInformation.MouseButtonsSwapped)
                User32.mouse_event((uint)User32.MouseEventFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
            else
                User32.mouse_event((uint)User32.MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);

            await Task.Delay(delay);

            //mouse up
            if (SystemInformation.MouseButtonsSwapped)
                User32.mouse_event((uint)User32.MouseEventFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
            else
                User32.mouse_event((uint)User32.MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);

            await Task.Delay(delay);
        }

        private async Task<Tuple<int, int>> GetCardsAmount(Card card,
            double cardPosX, double card2PosX, double cardPosY)
        {
            if (!User32.IsHearthstoneInForeground())
            {
                Log.WriteLine("Importing aborted, window lost focus", LogType.Info, LOGGER_CATEGORY);
                throw new ImportingException("Hearthstone window lost focus");
            }

            await ClickOnPoint(HearthstoneWindow, SearchBoxPosition, ImportStepDelay);

            string searchInput = Hearthstone_Deck_Tracker.Exporting.ExportingHelper.GetSearchString(card);
            if (PasteFromClipboard)
            {
                Clipboard.SetText(searchInput);
                SendKeys.SendWait("^v");
            }
            else
            {
                SendKeys.SendWait(searchInput);
            }
            SendKeys.SendWait("{ENTER}");

            Log.WriteLine("try to import card: " + card.Name, LogType.Debug, LOGGER_CATEGORY);
            await Task.Delay(ImportStepDelay * 3);

            Tuple<int, int> result;
            int posX = (int)cardPosX;
            int posY = (int)cardPosY;
            int posX2 = (int)card2PosX;

            if (CardExists(HearthstoneWindow, posX, posY))
            {
                int firstCardAmount = HasAmountLabel(HearthstoneWindow, posX, posY) ? 2 : 1;
                if (CardExists(HearthstoneWindow, posX2, posY))
                {
                    int secondCardAmount = HasAmountLabel(HearthstoneWindow, posX2, posY) ? 2 : 1;
                    result = new Tuple<int, int>(firstCardAmount, secondCardAmount);
                }
                else
                {
                    if (IsGoldenCard(HearthstoneWindow, posX, posY))
                    {
                        result = new Tuple<int, int>(0, firstCardAmount);
                    }
                    else
                    {
                        result = new Tuple<int, int>(firstCardAmount, 0);
                    }
                }
            }
            else
            {
                result = new Tuple<int, int>(0, 0);
            }

            return result;
        }

        private bool CardExists(IntPtr wndHandle, int posX, int posY)
        {
            const double initialSize = 40; // 40px @ height = 1080
            const double minHue = 90;

            int size = (int)Math.Round(initialSize * WindowYRatioTo1080);

            var capture = ScreenCapture.CaptureScreen(wndHandle, new Point(posX, posY), size, size);
            if (capture == null)
                return false;

            return capture.GetAvgHue() > minHue;
        }

        private bool HasAmountLabel(IntPtr wndHandle, int posX, int posY)
        {
            const int screen1080CardHeight = 310;
            const int screen1920DistanceBeforeAmountLabel = 70;
            const double initialSize = 40; // 40px @ height = 1080
            const double maxBrightness = 0.58;
            int size = (int)Math.Round(initialSize * WindowYRatioTo1080);

            int cardYBottom = posY + (int)(WindowYRatioTo1080 * screen1080CardHeight);
            int cardXMiddle = posX + (int)(WindowXRatioTo1920 * screen1920DistanceBeforeAmountLabel);

            var capture = ScreenCapture.CaptureScreen(wndHandle, new Point(cardXMiddle, cardYBottom), size, size);
            if (capture == null)
                return false;

            return capture.GetAvgBrightness() <= maxBrightness;
        }

        private bool IsGoldenCard(IntPtr wndHandle, int posX, int posY)
        {
            const double initialHeight = 40;
            const double initialWidth = 20;
            const double minBrightness = 0.55;
            const double initialWidthToRightCorner = 180;

            int height = (int)Math.Round(initialHeight * WindowYRatioTo1080);
            int width = (int)Math.Round(initialWidth * WindowXRatioTo1920);
            int widthToRightCorner = (int)Math.Round(initialWidthToRightCorner * WindowXRatioTo1920);
            // need to track border of a card
            int borderPosY = posY + (int)(height * 0.35);
            int borderPosX = posX + widthToRightCorner;

            var capture = ScreenCapture.CaptureScreen(wndHandle, new Point(borderPosX, borderPosY), width, height);
            if (capture == null)
                return false;

            return capture.GetAvgBrightness() > minBrightness;
        }
    }
}

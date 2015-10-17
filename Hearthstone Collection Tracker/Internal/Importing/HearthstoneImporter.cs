using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                HearthstoneWindow = User32.GetHearthstoneWindow();

                if (!User32.IsHearthstoneInForeground())
                {
                    //restore window and bring to foreground
                    User32.ShowWindow(HearthstoneWindow, User32.SwRestore);
                    User32.SetForegroundWindow(HearthstoneWindow);
                    //wait it to actually be in foreground, else the rect might be wrong
                    await Task.Delay(500);
                }
                if (!User32.IsHearthstoneInForeground())
                {
                    Logger.WriteLine("Can't find Hearthstone window.", LOGGER_CATEGORY);
                    throw new ImportingException("Can't find Hearthstone window.");
                }

                Logger.WriteLine("Waiting for " + ImportStepDelay * 3 + " milliseconds before starting the collection import", LOGGER_CATEGORY);
                await Task.Delay(ImportStepDelay * 3);

                var hsRect = User32.GetHearthstoneRect(false);
                WindowXRatioTo1920 = (double)hsRect.Width / 1920;
                WindowYRatioTo1080 = (double)hsRect.Height / 1080;
                var ratio = (4.0 / 3.0) / ((double)hsRect.Width / hsRect.Height);

                SearchBoxPosition = new Point((int)(GetXPos(Config.Instance.ExportSearchBoxX, hsRect.Width, ratio)),
                    (int)(Config.Instance.ExportSearchBoxY * hsRect.Height));
                var cardPosX = GetXPos(Config.Instance.ExportCard1X, hsRect.Width, ratio);
                var card2PosX = GetXPos(Config.Instance.ExportCard2X, hsRect.Width, ratio);
                var cardPosY = Config.Instance.ExportCardsY * hsRect.Height;

                foreach (var set in sets)
                {
                    foreach (var card in set.Cards)
                    {
                        var amount = await GetCardsAmount(card.Card, cardPosX, card2PosX, cardPosY);
                        int amountNonGolden = amount.Item1;
                        int amountGolden = amount.Item2;
                        if (NonGoldenFirst && amountNonGolden < card.MaxAmountInCollection && amountGolden > 0)
                        {
                            int missing = card.MaxAmountInCollection - amountNonGolden;
                            int transferAmount = Math.Min(amountGolden, missing);
                            amountGolden -= transferAmount;
                            amountNonGolden += transferAmount;
                        }
                        card.AmountNonGolden = Math.Min(amountNonGolden, card.MaxAmountInCollection);
                        card.AmountGolden = Math.Min(amountGolden, card.MaxAmountInCollection);
                    }
                }
            }
            catch (ImportingException e)
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
                Logger.WriteLine("Importing aborted, window lost focus", LOGGER_CATEGORY);
                throw new ImportingException("Hearthstone window lost focus");
            }

            await ClickOnPoint(HearthstoneWindow, SearchBoxPosition, ImportStepDelay);

            var addArtist = new[] { "zhCN", "zhTW", "ruRU", "koKR" }.All(x => Config.Instance.SelectedLanguage != x);
            var fixedName = addArtist ? (card.LocalizedName + " " + card.Artist).ToLowerInvariant()
                : card.LocalizedName.ToLowerInvariant();
            if (PasteFromClipboard)
            {
                Clipboard.SetText(fixedName);
                SendKeys.SendWait("^v");
            }
            else
            {
                SendKeys.SendWait(fixedName);
            }
            SendKeys.SendWait("{ENTER}");

            Logger.WriteLine("try to import card: " + card.Name, LOGGER_CATEGORY, 1);
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

            var capture = Helper.CaptureHearthstone(new Point(posX, posY), size, size, wndHandle);
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

            var capture = Helper.CaptureHearthstone(new Point(cardXMiddle, cardYBottom), size, size, wndHandle);
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

            var capture = Helper.CaptureHearthstone(new Point(borderPosX, borderPosY), width, height, wndHandle);
            if (capture == null)
                return false;

            return capture.GetAvgBrightness() > minBrightness;
        }
    }
}

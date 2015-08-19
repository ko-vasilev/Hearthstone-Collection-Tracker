using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Collection_Tracker.Internal.DataUpdaters;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hearthstone_Collection_Tracker
{
    public class HearthstoneCollectionTrackerPlugin : Hearthstone_Deck_Tracker.Plugins.IPlugin
    {
        public void OnLoad()
        {
            DefaultDataUpdater.PerformUpdates();

            Settings = PluginSettings.LoadSettings(PluginDataDir);

            // initialize image for icon
            var img = new Image();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(@"pack://application:,,,/HearthstoneCollectionTracker;component/Internal/HSCollection.ico", UriKind.RelativeOrAbsolute);
            bmp.EndInit();

            img.Source = bmp;
            img.Width = bmp.PixelWidth;

            MainMenuItem = new MenuItem
            {
                Header = Name,
                Icon = img
            };

            MainMenuItem.Click += (sender, args) =>
            {
                if (_mainWindow == null)
                {
                    InitializeMainWindow();
                    _mainWindow.Show();
                }
                else
                {
                    _mainWindow.Activate();
                }
            };

            Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckCreated.Add(HandleHearthstoneDeckUpdated);
            Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckUpdated.Add(HandleHearthstoneDeckUpdated);
        }

        private void HandleHearthstoneDeckUpdated(Deck deck)
        {
            if (deck == null || !Settings.NotifyNewDeckMissingCards)
                return;

            if (deck.IsArenaDeck)
                return;

            List<Tuple<Card, int>> missingCards = new List<Tuple<Card, int>>();

            foreach (var deckCard in deck.Cards)
            {
                var cardSet = Settings.ActiveAccountSetsInfo.FirstOrDefault(set => set.SetName == deckCard.Set);
                if (cardSet == null)
                {
                    continue;
                }
                var collectionCard = cardSet.Cards.FirstOrDefault(c => c.CardId == deckCard.Id);
                if (collectionCard == null)
                {
                    continue;
                }

                int missingAmount = Math.Max(0, deckCard.Count - (collectionCard.AmountGolden + collectionCard.AmountNonGolden));
                if (missingAmount > 0)
                {
                    missingCards.Add(new Tuple<Card, int>(deckCard, missingAmount));
                }
            }

            if (missingCards.Any())
            {
                StringBuilder alertSB = new StringBuilder();
                foreach (var gr in missingCards.GroupBy(c => c.Item1.Set))
                {
                    alertSB.AppendFormat("{0} set:", gr.Key);
                    alertSB.AppendLine();
                    foreach(var card in gr)
                    {
                        alertSB.AppendFormat("  • {0} ({1});", card.Item1.LocalizedName, card.Item2);
                        alertSB.AppendLine();
                    }
                }
                alertSB.Append("You can disable this alert in Collection Tracker plugin settings.");
                Hearthstone_Deck_Tracker.Helper.MainWindow.ShowMessageAsync("Missing cards in collection", alertSB.ToString());
            }
        }

        public void OnUnload()
        {
            if (_mainWindow != null)
            {
                if (_mainWindow.IsVisible)
                {
                    _mainWindow.Close();
                }
                _mainWindow = null;
            }
            if (_settingsWindow != null)
            {
                if (_settingsWindow.IsVisible)
                {
                    _settingsWindow.Close();
                }
                _settingsWindow = null;
            }
            Settings.SaveSettings(PluginDataDir);
        }

        public void OnButtonPress()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(Settings);
                _settingsWindow.PluginWindow = _mainWindow;
                _settingsWindow.Closed += (sender, args) =>
                {
                    _settingsWindow = null;
                };
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        public void OnUpdate()
        {
            CheckForUpdates();
        }

        public string Name
        {
            get { return "Collection Tracker"; }
        }

        public string Description
        {
            get
            {
                return @"Helps user to keep track on packs progess, suggesting the packs that will most probably contain missing cards.
Suggestions and bug reports can be sent to https://github.com/ko-vasilev/Hearthstone-Deck-Tracker or directly to e-mail oppa.kostya.bko@gmail.com.";
            }
        }

        public string ButtonText
        {
            get { return "Settings"; }
        }

        public string Author
        {
            get { return "Vasilev Konstantin"; }
        }

        public static readonly Version PluginVersion = new Version("0.1");

        public Version Version
        {
            get { return PluginVersion; }
        }

        protected MenuItem MainMenuItem { get; set; }

        protected static MainWindow _mainWindow;

        protected SettingsWindow _settingsWindow;

        protected void InitializeMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Width = Settings.CollectionWindowWidth;
                _mainWindow.Filter.OnlyMissing = !Settings.DefaultShowAllCards;
                _mainWindow.Closed += (sender, args) =>
                {
                    Settings.CollectionWindowWidth = _mainWindow.Width;
                    if (_mainWindow.Filter != null)
                    {
                        Settings.DefaultShowAllCards = !_mainWindow.Filter.OnlyMissing;
                    }
                    _mainWindow = null;
                };
            }
        }

        public MenuItem MenuItem
        {
            get { return MainMenuItem; }
        }

        internal static string PluginDataDir
        {
            get { return System.IO.Path.Combine(Hearthstone_Deck_Tracker.Config.Instance.DataDir, "CollectionTracker");  }
        }

        internal static PluginSettings Settings { get; set; }

        #region Auto Update check implementation

        private DateTime _lastTimeUpdateChecked = DateTime.MinValue;

        private readonly TimeSpan _updateCheckInterval = TimeSpan.FromHours(1);

        private bool _hasUpdates = false;

        private bool _showingUpdateMessage = false;

        private async Task CheckForUpdates()
        {
            if (!_hasUpdates)
            {
                if ((DateTime.Now - _lastTimeUpdateChecked) > _updateCheckInterval)
                {
                    _lastTimeUpdateChecked = DateTime.Now;
                    var latestVersion = await Helpers.GetLatestVersion();
                    _hasUpdates = latestVersion > Version;
                }
            }

            if (_hasUpdates)
            {
                if (!Game.IsRunning && _mainWindow != null && !_showingUpdateMessage)
                {
                    _showingUpdateMessage = true;
                    const string releaseDownloadUrl = @"https://github.com/ko-vasilev/Hearthstone-Collection-Tracker/releases/latest";
                    var settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "Not now"};

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        if (_mainWindow != null)
                        {
                            var result = await _mainWindow.ShowMessageAsync("New Update available!",
                                "Do you want to download it?",
                                MessageDialogStyle.AffirmativeAndNegative, settings);

                            if (result == MessageDialogResult.Affirmative)
                            {
                                Process.Start(releaseDownloadUrl);
                            }
                            _hasUpdates = false;
                            _lastTimeUpdateChecked = DateTime.Now.AddDays(1);
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _showingUpdateMessage = false;
                    }
                }
            }
        }

        #endregion
    }
}

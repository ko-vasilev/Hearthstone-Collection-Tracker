using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Collection_Tracker
{
    public class HearthstoneCollectionTrackerPlugin : IPlugin
    {
        public void OnLoad()
        {
            MainMenuItem = new MenuItem
            {
                Header = ButtonText
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
        }

        public void OnButtonPress()
        {
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
            get { return "Collection Tracker"; }
        }

        public string Author
        {
            get { return "Vasilev Konstantin"; }
        }

        public Version Version
        {
            get { return new Version("0.1"); }
        }

        protected MenuItem MainMenuItem { get; set; }

        protected static MainWindow _mainWindow;

        protected void InitializeMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closed += (sender, args) =>
                {
                    _mainWindow = null;
                };
            }
        }

        public MenuItem MenuItem
        {
            get { return MainMenuItem; }
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

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
    }
}

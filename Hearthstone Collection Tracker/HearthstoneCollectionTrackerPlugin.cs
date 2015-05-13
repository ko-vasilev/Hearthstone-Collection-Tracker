using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;

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
    }
}

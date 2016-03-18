using System;
using System.Collections.Generic;
using System.IO;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV032 : IDataUpdater
    {
        private static readonly Version _version = new Version(0, 3, 2);

        public Version Version
        {
            get
            {
                return _version;
            }
        }

        private string ConfigFilePath
        {
            get { return Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "config.xml"); }
        }

        public bool RequiresUpdate
        {
            get
            {
                var configFilePath = ConfigFilePath;
                if (!Directory.Exists(HearthstoneCollectionTrackerPlugin.PluginDataDir) || !File.Exists(configFilePath))
                {
                    return false;
                }

                try
                {
                    var settings = Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Load(configFilePath);
                    return settings.CurrentVersion < new ModuleVersion(_version);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public void PerformUpdate()
        {
            var configFilePath = ConfigFilePath;
            var settings = Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Load(configFilePath);
            settings.CurrentVersion = new ModuleVersion(_version);
            Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Save(configFilePath, settings);
        }

        [Serializable]
        public class PluginSettings
        {
            public ModuleVersion CurrentVersion { get; set; }

            public string ActiveAccount { get; set; }

            public List<AccountSummary> Accounts { get; set; }

            public double CollectionWindowWidth { get; set; }

            public double CollectionWindowHeight { get; set; }

            public bool DefaultShowAllCards { get; set; }

            public bool NotifyNewDeckMissingCards { get; set; }

            public bool EnableDesiredCardsFeature { get; set; }
        }
    }
}

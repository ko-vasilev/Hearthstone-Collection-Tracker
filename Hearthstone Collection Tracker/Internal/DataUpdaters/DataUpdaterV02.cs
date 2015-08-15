using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV02 : IDataUpdater
    {
        public Version Version
        {
            get { return _version; }
        }

        public bool RequiresUpdate
        {
            get
            {
                return !Directory.Exists(HearthstoneCollectionTrackerPlugin.PluginDataDir);
            }
        }

        private static readonly Version _version = new Version(0, 2);

        public void PerformUpdate()
        {
            Directory.CreateDirectory(HearthstoneCollectionTrackerPlugin.PluginDataDir);

            // move collection info file into data dir
            string oldCollectionFilePath = Path.Combine(Hearthstone_Deck_Tracker.Config.Instance.DataDir, "CardCollection.xml");
            string newCollectionFilePath = Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_Default.xml");
            if (File.Exists(oldCollectionFilePath))
            {
                File.Move(oldCollectionFilePath, newCollectionFilePath);
            }

            PluginSettings settings = new PluginSettings()
            {
                CurrentVersion = new ModuleVersion(_version),
                Accounts = new List<AccountSummary>()
                    {
                        new AccountSummary()
                        {
                            AccountName = "Default",
                            FileStoragePath = newCollectionFilePath
                        }
                    },
                ActiveAccount = "Default",
                CollectionWindowWidth = 300
            };
            string settingsFilePath = Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "config.xml");

            Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Save(settingsFilePath, settings);
        }

        [Serializable]
        public class PluginSettings
        {
            public ModuleVersion CurrentVersion { get; set; }

            public string ActiveAccount { get; set; }

            public List<AccountSummary> Accounts { get; set; }

            public double CollectionWindowWidth { get; set; }
        }

        [Serializable]
        public class AccountSummary
        {
            public string AccountName { get; set; }

            public string FileStoragePath { get; set; }
        }
    }
}

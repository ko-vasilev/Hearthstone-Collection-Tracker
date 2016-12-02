using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    class DataUpdaterV051 : IDataUpdater
    {
        private static readonly Version _version = new Version(0, 5, 1);

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
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void PerformUpdate()
        {
            const string MSoGSet = "Mean Streets of Gadgetzan";
            // iterate over each collection and add MSoG cards
            foreach (var file in Directory.GetFiles(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_*.xml", SearchOption.TopDirectoryOnly))
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(file);
                if (setInfos.Any(s => s.SetName == MSoGSet))
                    continue;
                var cards = Hearthstone_Deck_Tracker.Hearthstone.Database.GetActualCards();
                // add MSoG cards
                setInfos.Add(new BasicSetCollectionInfo()
                {
                    SetName = MSoGSet,
                    Cards = cards.Where(c => c.Set == MSoGSet).Select(c => new CardInCollection()
                    {
                        AmountGolden = 0,
                        AmountNonGolden = 0,
                        CardId = c.Id,
                        DesiredAmount = c.Rarity == Rarity.LEGENDARY ? 1 : 2
                    }).ToList()
                });
                XmlManager<List<BasicSetCollectionInfo>>.Save(file, setInfos);
            }

            var configFilePath = ConfigFilePath;
            var settings = XmlManager<PluginSettings>.Load(configFilePath);
            settings.CurrentVersion = new ModuleVersion(_version);
            XmlManager<PluginSettings>.Save(configFilePath, settings);
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

        public class BasicSetCollectionInfo
        {
            public string SetName { get; set; }

            public List<CardInCollection> Cards { get; set; }
        }

        public class CardInCollection
        {
            public int AmountNonGolden { get; set; }

            public int AmountGolden { get; set; }

            public int DesiredAmount { get; set; }

            public string CardId { get; set; }
        }
    }
}

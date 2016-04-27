using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HearthDb.Enums;

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
                List<BasicSetCollectionInfo> oldSetInfo = Hearthstone_Deck_Tracker.XmlManager<List<BasicSetCollectionInfo>>.Load(oldCollectionFilePath);
                var cards = Hearthstone_Deck_Tracker.Hearthstone.Database.GetActualCards();
                foreach (var set in oldSetInfo)
                {
                    foreach (var card in set.Cards)
                    {
                        var originalCard = cards.FirstOrDefault(c => c.Id == card.CardId);
                        if (originalCard != null)
                        {
                            card.DesiredAmount = originalCard.Rarity == Rarity.LEGENDARY ? 1 : 2;
                        }
                    }
                }
                // add TGT cards
                const string TGTSet = "The Grand Tournament";
                oldSetInfo.Add(new BasicSetCollectionInfo()
                {
                    SetName = TGTSet,
                    Cards = cards.Where(c => c.Set == TGTSet).Select(c => new CardInCollection()
                    {
                        AmountGolden = 0,
                        AmountNonGolden = 0,
                        CardId = c.Id,
                        DesiredAmount = c.Rarity == Rarity.LEGENDARY ? 1 : 2
                    }).ToList()
                });
                Hearthstone_Deck_Tracker.XmlManager<List<BasicSetCollectionInfo>>.Save(newCollectionFilePath, oldSetInfo);
                File.Delete(oldCollectionFilePath);
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
                CollectionWindowWidth = 395,
                CollectionWindowHeight = 560,
                DefaultShowAllCards = false,
                NotifyNewDeckMissingCards = true
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

            public double CollectionWindowHeight { get; set; }

            public bool DefaultShowAllCards { get; set; }

            public bool NotifyNewDeckMissingCards { get; set; }
        }

        [Serializable]
        public class AccountSummary
        {
            public string AccountName { get; set; }

            public string FileStoragePath { get; set; }
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

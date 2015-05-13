using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hearthstone_Collection_Tracker
{
    internal class SetCardsManager
    {
        private readonly string[] CollectableSets = { "Classic", "Goblins vs Gnomes" };

        private string StorageFilePath
        {
            get { return Config.Instance.DataDir + "CardCollection.xml"; }
        }

        public List<BasicSetCollectionInfo> SetCards { get; private set; }

        protected SetCardsManager()
        {
        }

        private static SetCardsManager _instance;

        public static SetCardsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SetCardsManager();
                    _instance.LoadSetsInfo();
                }
                return _instance;
            }
        }

        protected void LoadSetsInfo()
        {
            bool infoLoadedFromFile = false;
            var cards = Game.GetActualCards();
            if (File.Exists(StorageFilePath))
            {
                try
                {
                    var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(StorageFilePath);
                    if (setInfos != null)
                    {
                        SetCards = setInfos;
                        foreach (var setCollection in SetCards)
                        {
                            foreach (var card in setCollection.Cards)
                            {
                                card.Card = cards.First(c => c.Id == card.CardId);
                                card.AmountGolden = card.AmountGolden.Clamp(0, card.MaxAmountInCollection);
                                card.AmountNonGolden = card.AmountNonGolden.Clamp(0, card.MaxAmountInCollection);

                            }
                        }
                        infoLoadedFromFile = true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("File with your collection information is corrupted.");
                }
            }
            if (!infoLoadedFromFile)
            {
                SetCards = CollectableSets.Select(set => new BasicSetCollectionInfo()
                {
                    SetName = set,
                    Cards = cards.Where(c => c.Set == set)
                        .Select(c => new CardInCollection(c))
                        .ToList()
                }).ToList();
            }
        }

        public void SaveCollection(List<BasicSetCollectionInfo> collections)
        {
            XmlManager<List<BasicSetCollectionInfo>>.Save(StorageFilePath, collections);
        }
    }
}

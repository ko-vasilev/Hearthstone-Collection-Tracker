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
    internal static class SetCardsManager
    {
        private static readonly string[] CollectableSets = { "Classic", "Goblins vs Gnomes" };

        public static List<BasicSetCollectionInfo> LoadSetsInfo(string collectionStoragePath)
        {
            List<BasicSetCollectionInfo> collection = null;
            try
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(collectionStoragePath);
                if (setInfos != null)
                {
                    var cards = Game.GetActualCards();
                    collection = setInfos;
                    foreach (var setCollection in collection)
                    {
                        foreach (var card in setCollection.Cards)
                        {
                            card.Card = cards.First(c => c.Id == card.CardId);
                            card.AmountGolden = card.AmountGolden.Clamp(0, card.MaxAmountInCollection);
                            card.AmountNonGolden = card.AmountNonGolden.Clamp(0, card.MaxAmountInCollection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File with your collection information is corrupted.");
            }
            return collection;
        }

        public static List<BasicSetCollectionInfo> CreateEmptyCollection()
        {
            var cards = Game.GetActualCards();
            var setCards = CollectableSets.Select(set => new BasicSetCollectionInfo()
            {
                SetName = set,
                Cards = cards.Where(c => c.Set == set)
                        .Select(c => new CardInCollection(c))
                        .ToList()
            }).ToList();
            return setCards;
        }

        public static void SaveCollection(List<BasicSetCollectionInfo> collections, string saveFilePath)
        {
            XmlManager<List<BasicSetCollectionInfo>>.Save(saveFilePath, collections);
        }
    }
}

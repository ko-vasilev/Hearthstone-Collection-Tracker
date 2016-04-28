using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Collection_Tracker
{
    internal static class SetCardsManager
    {
        public static readonly string[] CollectableSets = { "Classic", "Goblins vs Gnomes", "The Grand Tournament", "Whispers of the Old Gods" };

        public static List<BasicSetCollectionInfo> LoadSetsInfo(string collectionStoragePath)
        {
            List<BasicSetCollectionInfo> collection = null;
            try
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(collectionStoragePath);
                if (setInfos != null)
                {
                    var cards = Database.GetActualCards();
                    collection = setInfos;
                    foreach (var set in CollectableSets)
                    {
                        var setInfo = setInfos.FirstOrDefault(si => si.SetName.Equals(set, StringComparison.InvariantCultureIgnoreCase));
                        if (setInfo == null)
                        {
                            collection.Add(new BasicSetCollectionInfo()
                            {
                                SetName = set,
                                Cards = cards.Where(c => c.Set.Equals(set, StringComparison.InvariantCultureIgnoreCase))
                                            .Select(c => new CardInCollection()
                                            {
                                                AmountGolden = 0,
                                                AmountNonGolden = 0,
                                                Card = c,
                                                CardId = c.Id,
                                                DesiredAmount = CardInCollection.GetMaxAmountInCollection(c.Rarity)
                                            })
                                            .ToList()
                            });
                        }
                        else
                        {
                            foreach (var card in setInfo.Cards)
                            {
                                card.Card = cards.First(c => c.Id == card.CardId);
                                card.AmountGolden = card.AmountGolden.Clamp(0, card.MaxAmountInCollection);
                                card.AmountNonGolden = card.AmountNonGolden.Clamp(0, card.MaxAmountInCollection);
                            }
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
            var cards = Database.GetActualCards();
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

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
                        var currentSetCards = cards.Where(c => c.Set.Equals(set, StringComparison.InvariantCultureIgnoreCase));
                        var setInfo = setInfos.FirstOrDefault(si => si.SetName.Equals(set, StringComparison.InvariantCultureIgnoreCase));
                        if (setInfo == null)
                        {
                            collection.Add(new BasicSetCollectionInfo()
                            {
                                SetName = set,
                                Cards = currentSetCards.Select(c => new CardInCollection(c)).ToList()
                            });
                        }
                        else
                        {
                            foreach (var card in currentSetCards)
                            {
                                var savedCard = setInfo.Cards.FirstOrDefault(c => c.CardId == card.Id);
                                if (savedCard == null)
                                {
                                    setInfo.Cards.Add(new CardInCollection(card));
                                }
                                else
                                {
                                    savedCard.Card = card;
                                    savedCard.AmountGolden = savedCard.AmountGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                    savedCard.AmountNonGolden = savedCard.AmountNonGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File with your collection information is corrupted.", ex);
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

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
        public static readonly string[] CollectableSets = { "Classic", "Curse of Naxxramas", "Goblins vs Gnomes", "Blackrock Mountain", "The Grand Tournament" };

        public static List<BasicSetCollectionInfo> LoadSetsInfo(string collectionStoragePath)
        {
            // Usable list of sets to update the collection with missing ones
            var sets = CreateEmptyCollection();

            List<BasicSetCollectionInfo> collection = null;
            try
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(collectionStoragePath);
                if (setInfos != null)
                {
                    var cards = Database.GetActualCards();
                    collection = setInfos;
                    foreach (var setCollection in collection)
                    {
                        // Remove the current set from listSets
                        int index = sets.FindIndex(x => x.SetName == setCollection.SetName);
                        if (index >= 0)
                        {
                            sets[index] = null;
                            sets.RemoveAt(index);
                        }

                        foreach (var card in setCollection.Cards)
                        {
                            card.Card = cards.First(c => c.Id == card.CardId);
                            card.AmountGolden = card.AmountGolden.Clamp(0, card.MaxAmountInCollection);
                            card.AmountNonGolden = card.AmountNonGolden.Clamp(0, card.MaxAmountInCollection);
                        }
                    }

                    // Adding the sets that aren't already in the collection (in the right order)
                    int indexDest = 0;
                    foreach (var setName in CollectableSets)
                    {
                        int indexSrc = sets.FindIndex(x => x.SetName == setName);
                        if (indexSrc >= 0)
                            collection.Insert(indexDest, sets[indexSrc]);
                        indexDest++;
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

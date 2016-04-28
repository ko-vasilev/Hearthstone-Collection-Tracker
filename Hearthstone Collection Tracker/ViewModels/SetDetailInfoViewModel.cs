using Hearthstone_Collection_Tracker.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using CRarity = HearthDb.Enums.Rarity;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class SetDetailInfoViewModel : DependencyObject, INotifyPropertyChanged
    {
        public SetDetailInfoViewModel()
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SetCards")
                {
                    List<CardStatsByRarity> cardStats = SetCards.GroupBy(c => c.Card.Rarity, c => c)
                        .OrderBy(g => (int)g.Key)
                        .Select(gr => new CardStatsByRarity(gr.Key.ToString(), gr.AsEnumerable()))
                        .ToList();
                    TotalSetStats = new CardStatsByRarity("Total", SetCards);
                    cardStats.Add(TotalSetStats);
                    StatsByRarity = cardStats;
                }
            };
        }

        #region Properties

        public string SetName { get; set; }

        public static readonly DependencyProperty SetCardsProperty = DependencyProperty.Register("SetCards",
            typeof(TrulyObservableCollection<CardInCollection>), typeof(SetDetailInfoViewModel),
            new PropertyMetadata(new TrulyObservableCollection<CardInCollection>()));

        public TrulyObservableCollection<CardInCollection> SetCards
        {
            get { return (TrulyObservableCollection<CardInCollection>)GetValue(SetCardsProperty); }
            set
            {
                if (SetCards != null)
                {
                    SetCards.CollectionChanged -= NotifySetCardsChanged;
                }
                SetValue(SetCardsProperty, value);
                if (value != null)
                {
                    value.CollectionChanged += NotifySetCardsChanged;
                    OnPropertyChanged("SetCards");
                }
            }
        }

        public static readonly DependencyProperty StatsByRarityProperty = DependencyProperty.Register("StatsByRarity",
            typeof(IEnumerable<CardStatsByRarity>), typeof(SetDetailInfoViewModel));

        public IEnumerable<CardStatsByRarity> StatsByRarity
        {
            get { return (IEnumerable<CardStatsByRarity>)GetValue(StatsByRarityProperty); }
            private set { SetValue(StatsByRarityProperty, value); }
        }

        public static readonly DependencyProperty TotalSetStatsProperty = DependencyProperty.Register("TotalSetStats",
    typeof(CardStatsByRarity), typeof(SetDetailInfoViewModel));

        public CardStatsByRarity TotalSetStats
        {
            get { return (CardStatsByRarity)GetValue(TotalSetStatsProperty); }
            private set { SetValue(TotalSetStatsProperty, value); }
        }

        #endregion

        private void NotifySetCardsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged("SetCards");
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class CardStatsByRarity
    {
        #region Static cards info
        private static readonly ReadOnlyDictionary<CRarity, double> CardProbabilities = new ReadOnlyDictionary<CRarity, double>(
            new Dictionary<CRarity, double>
            {
                { CRarity.COMMON, 0.6997 },
                { CRarity.RARE, 0.2140 },
                { CRarity.EPIC, 0.0429 },
                { CRarity.LEGENDARY, 0.0108 }
            });

        private static readonly ReadOnlyDictionary<CRarity, double> GoldenCardProbabilities = new ReadOnlyDictionary<CRarity, double>(
            new Dictionary<CRarity, double>
            {
                { CRarity.COMMON, 0.0146 },
                { CRarity.RARE, 0.0138 },
                { CRarity.EPIC, 0.0031 },
                { CRarity.LEGENDARY, 0.0011 }
            });

        /// <summary>
        /// contains info for both golden and non-golden cards
        /// </summary>
        private static readonly ReadOnlyDictionary<CRarity, double> AllCardProbabilitiesByRarity = new ReadOnlyDictionary<CRarity, double>(
            new Dictionary<CRarity, double>
            {
                { CRarity.COMMON, 0.7143 },
                { CRarity.RARE, 0.2278 },
                { CRarity.EPIC, 0.046 },
                { CRarity.LEGENDARY, 0.0119 }
            });

        private static readonly ReadOnlyDictionary<CRarity, int> CardCraftValue = new ReadOnlyDictionary<CRarity, int>(
    new Dictionary<CRarity, int>
            {
                { CRarity.COMMON, 40 },
                { CRarity.RARE, 100 },
                { CRarity.EPIC, 400 },
                { CRarity.LEGENDARY, 1600 }
            });

        private static readonly ReadOnlyDictionary<CRarity, int> GoldenCardCraftValue = new ReadOnlyDictionary<CRarity, int>(
    new Dictionary<CRarity, int>
            {
                { CRarity.COMMON, 400 },
                { CRarity.RARE, 800 },
                { CRarity.EPIC, 1600 },
                { CRarity.LEGENDARY, 3200 }
            });

        private static readonly ReadOnlyDictionary<CRarity, int> CardDisenchantValue = new ReadOnlyDictionary<CRarity, int>(
new Dictionary<CRarity, int>
            {
                { CRarity.COMMON, 5 },
                { CRarity.RARE, 20 },
                { CRarity.EPIC, 100 },
                { CRarity.LEGENDARY, 400 }
            });

        private static readonly ReadOnlyDictionary<CRarity, int> GoldenCardDisenchantValue = new ReadOnlyDictionary<CRarity, int>(
    new Dictionary<CRarity, int>
            {
                { CRarity.COMMON, 50 },
                { CRarity.RARE, 100 },
                { CRarity.EPIC, 400 },
                { CRarity.LEGENDARY, 1600 }
            });
        #endregion

        public CardStatsByRarity()
        {
            TotalDesiredAmount = 0;
            TotalAmount = 0;
            PlayerHas = 0;
            PlayerHasGolden = 0;
            PlayerHasDesired = 0;
            MissingDesiredAmount = 0;
        }

        public CardStatsByRarity(string rarity, IEnumerable<CardInCollection> cards)
            : this()
        {
            _cards = cards;
            Rarity = rarity;

            foreach(var card in cards)
            {
                TotalDesiredAmount += card.DesiredAmount;
                TotalAmount += card.MaxAmountInCollection;
                PlayerHas += card.AmountNonGolden;
                PlayerHasGolden += card.AmountGolden;
                PlayerHasDesired += Math.Min(card.AmountGolden + card.AmountNonGolden, card.DesiredAmount);
            }
            MissingDesiredAmount = TotalDesiredAmount - PlayerHasDesired;

            OpenGoldenOdds = CalculateOpeningOdds(cards, card => card.MaxAmountInCollection - card.AmountGolden, GoldenCardProbabilities);
            OpenNonGoldenOdds = CalculateOpeningOdds(cards, card => card.MaxAmountInCollection - card.AmountNonGolden, CardProbabilities);
            OpenDesiredOdds = CalculateOpeningOdds(cards, card => Math.Max(0, card.DesiredAmount - (card.AmountGolden + card.AmountNonGolden)), AllCardProbabilitiesByRarity);
        }

        private const int CARDS_IN_PACK = 5;

        public string Rarity { get; set; }

        public int TotalDesiredAmount { get; set; }

        public int TotalAmount { get; set; }

        public int PlayerHas { get; set; }

        public int PlayerHasGolden { get; set; }

        public int PlayerHasDesired { get; set; }

        public int MissingDesiredAmount { get; set; }

        public double OpenGoldenOdds { get; set; }

        public double OpenNonGoldenOdds { get; set; }

        public double OpenDesiredOdds { get; set; }

        public double AverageDustValue
        {
            get
            {
                double totalAvgDustValue = 0;
                foreach (var group in _cards.GroupBy(c => c.Card.Rarity))
                {
                    var currentRarity = group.Key;
                    int maxCardsAmount = group.Sum(c => c.MaxAmountInCollection);

                    int havingNonGolden = group.Sum(c => c.AmountNonGolden);
                    double nonGoldenAverageValue = ((double)havingNonGolden / maxCardsAmount)
                        * CardDisenchantValue[currentRarity] * CardProbabilities[currentRarity];

                    int havingGolden = group.Sum(c => c.AmountGolden);
                    double goldenAverageValue = ((double)havingGolden / maxCardsAmount)
                        * GoldenCardDisenchantValue[currentRarity] * GoldenCardProbabilities[currentRarity];

                    totalAvgDustValue += nonGoldenAverageValue + goldenAverageValue;
                }

                return totalAvgDustValue * CARDS_IN_PACK;
            }
        }

        public double AverageDustValueNonDesired
        {
            get
            {
                double totalAvgDustValue = 0;
                foreach(var group in _cards.GroupBy(c => c.Card.Rarity))
                {
                    var currentRarity = group.Key;
                    int maxCardsAmount = group.Sum(c => c.MaxAmountInCollection);

                    int disenchantingCards = group.Sum(c => Math.Min(c.AmountGolden + c.AmountNonGolden + (c.MaxAmountInCollection - c.DesiredAmount), c.MaxAmountInCollection));
                    double nonGoldenAverageValue = ((double)disenchantingCards / maxCardsAmount)
                        * CardDisenchantValue[currentRarity] * CardProbabilities[currentRarity];
                    double goldenAverageValue = ((double)disenchantingCards / maxCardsAmount)
                        * GoldenCardDisenchantValue[currentRarity] * GoldenCardProbabilities[currentRarity];

                    totalAvgDustValue += nonGoldenAverageValue + goldenAverageValue;
                }

                return totalAvgDustValue * CARDS_IN_PACK;
            }
        }

        public double CraftNonGoldenDustRequired
        {
            get
            {
                return CalculateCardsCraftRequiredDust(CardCraftValue, card => card.MaxAmountInCollection - card.AmountNonGolden);
            }
        }

        public double CraftGoldenDustRequired
        {
            get
            {
                return CalculateCardsCraftRequiredDust(GoldenCardCraftValue, card => card.MaxAmountInCollection - card.AmountGolden);
            }
        }

        private IEnumerable<CardInCollection> _cards { get; set; }

        private double CalculateOpeningOdds(IEnumerable<CardInCollection> cards, Func<CardInCollection, int> cardsAmount, IDictionary<CRarity, double> probabilities)
        {
            double oddsForAllRaritites = 0.0;
            foreach (var group in cards.GroupBy(c => c.Card.Rarity, c => new { card = c, amount = cardsAmount(c) }))
            {
                double currentProbability = probabilities[group.Key];
                int missingCardsAmount = group.Sum(c => Math.Min(1, c.amount));
                int totalCardsAmount = group.Count();
                double missingCardsOdds = (double)missingCardsAmount / totalCardsAmount;

                oddsForAllRaritites += currentProbability * missingCardsOdds;
            }

            double resultOdds = 1 - Math.Pow(1 - oddsForAllRaritites, CARDS_IN_PACK);
            return resultOdds;
        }

        private double CalculateCardsCraftRequiredDust(IDictionary<CRarity, int> cardsCraftValue, Func<CardInCollection, int> missingCardsCount)
        {
            double totalRequiredDust = 0;
            foreach (var group in _cards.GroupBy(c => c.Card.Rarity))
            {
                var currentRarity = group.Key;
                double missingCards = group.Sum(missingCardsCount);
                totalRequiredDust += cardsCraftValue[currentRarity] * missingCards;
            }
            return totalRequiredDust;
        }
    }
}

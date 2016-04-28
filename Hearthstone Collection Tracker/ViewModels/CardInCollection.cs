using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using HearthDb.Enums;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class CardInCollection : INotifyPropertyChanged
    {
        public CardInCollection() { }

        public CardInCollection(Card card, int amountNonGolden = 0, int amountGolden = 0)
        {
            Card = card;
            AmountNonGolden = amountNonGolden;
            AmountGolden = amountGolden;
            DesiredAmount = MaxAmountInCollection;
            CardId = card.Id;
        }

        [XmlIgnore]
        public Card Card { get; set; }

        private int _amountNonGolden;

        public int AmountNonGolden
        {
            get { return _amountNonGolden; }
            set
            {
                _amountNonGolden = value;
                OnPropertyChanged();
            }
        }

        private int _amountGolden;

        public int AmountGolden
        {
            get { return _amountGolden; }
            set
            {
                _amountGolden = value;
                OnPropertyChanged();
            }
        }

        public static int GetMaxAmountInCollection(Rarity rarity)
        {
            return rarity == Rarity.LEGENDARY ? 1 : 2;
        }

        public int MaxAmountInCollection
        {
            get
            {
                if (Card == null)
                    throw new ArgumentNullException();
                return GetMaxAmountInCollection(Card.Rarity);
            }
        }

        private int _desiredAmount;

        public int DesiredAmount
        {
            get { return _desiredAmount; }
            set
            {
                _desiredAmount = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<int> DesiredAmountOptions
        {
            get
            {
                return Enumerable.Range(0, MaxAmountInCollection + 1);
            }
        }

        public string CardClass
        {
            get { return Card == null ? string.Empty : Card.GetPlayerClass; }
        }

        private string _cardId;
        public string CardId
        {
            get { return Card == null ? _cardId : Card.Id; }
            set { _cardId = value; }
        }

        #region INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

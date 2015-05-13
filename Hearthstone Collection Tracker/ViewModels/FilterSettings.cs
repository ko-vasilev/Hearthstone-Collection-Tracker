using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class FilterSettings : INotifyPropertyChanged
    {
        public FilterSettings()
        {
            OnlyMissing = true;
            GoldenCards = false;
            Text = string.Empty;
        }

        private bool _onlyMissing;

        public bool OnlyMissing
        {
            get
            {
                return _onlyMissing;
            }
            set
            {
                _onlyMissing = value;
                OnPropertyChanged();
            }
        }

        private bool _goldenCards;

        public bool GoldenCards
        {
            get
            {
                return _goldenCards;
            }
            set
            {
                _goldenCards = value;
                OnPropertyChanged();
            }
        }

        private string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                FormattedText = value == null ? null : Helper.RemoveDiacritics(value.ToLowerInvariant(), true);
                OnPropertyChanged();
            }
        }

        public string FormattedText { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

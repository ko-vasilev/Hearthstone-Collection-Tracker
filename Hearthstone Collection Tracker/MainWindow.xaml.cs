using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Thickness TitleBarMargin
        {
            get { return new Thickness(0, TitlebarHeight, 0, 0); }
        }

        public IEnumerable<SetDetailInfoViewModel> SetsInfo { get; set; }

        public MainWindow()
        {
            SetsInfo = SetCardsManager.Instance.SetCards.Select(set => new SetDetailInfoViewModel
            {
                SetName = set.SetName,
                SetCards = new TrulyObservableCollection<CardInCollection>(set.Cards.ToList())
            });

            InitializeComponent();

            Filter = new FilterSettings();
            Filter.PropertyChanged += (sender, args) =>
            {
                HandleFilterChange(sender, args);
            };
        }

        private void EditCollection(SetDetailInfoViewModel setInfo)
        {
            CardCollectionEditor.ItemsSource = setInfo.SetCards;

            OpenCollectionFlyout();
        }

        #region Collection management

        public FilterSettings Filter { get; set; }

        private void OpenCollectionFlyout()
        {
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource);
            view.Filter = CardsFilter;
            if (!view.GroupDescriptions.Any())
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription("CardClass"));
            }
            view.CustomSort = new CardInCollectionComparer();

            FlyoutCollection.IsOpen = true;
        }

        private bool CardsFilter(object card)
        {
            CardInCollection c = card as CardInCollection;
            if (Filter.OnlyMissing)
            {
                if ((Filter.GoldenCards && c.AmountGolden >= c.MaxAmountInCollection)
                    || (!Filter.GoldenCards && c.AmountNonGolden >= c.MaxAmountInCollection))
                {
                    return false;
                }
            }
            if (Filter.FormattedText == string.Empty)
                return true;
            var cardName = Helper.RemoveDiacritics(c.Card.LocalizedName.ToLowerInvariant(), true);
            return cardName.Contains(Filter.FormattedText);
        }

        private CancellationTokenSource _filterCancel = new CancellationTokenSource();

        private async Task HandleFilterChange(object sender, PropertyChangedEventArgs args)
        {
            if (_filterCancel != null && !_filterCancel.IsCancellationRequested)
            {
                _filterCancel.Cancel();
            }

            if (args.PropertyName == "Text")
            {
                if (Filter.Text.Length < 4)
                {
                    // wait 300 ms before filtering
                    _filterCancel = new CancellationTokenSource();
                    Task t = Task.Delay(TimeSpan.FromMilliseconds(300), _filterCancel.Token);
                    await t;
                }
                FilterCollection();
            }
            else
            {
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            if (CardCollectionEditor.ItemsSource != null)
            {
                CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource).Refresh();
            }
        }

        private void TextBoxCollectionFilter_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var index = CardCollectionEditor.SelectedIndex;
            CardInCollection card = null;
            switch (e.Key)
            {
                case Key.Enter:
                    if (CardCollectionEditor.SelectedItem != null)
                        card = (CardInCollection)CardCollectionEditor.SelectedItem;
                    else if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D1:
                    if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D2:
                    if (CardCollectionEditor.Items.Count > 1)
                        card = (CardInCollection)CardCollectionEditor.Items[1];
                    break;
                case Key.D3:
                    if (CardCollectionEditor.Items.Count > 2)
                        card = (CardInCollection)CardCollectionEditor.Items[2];
                    break;
                case Key.D4:
                    if (CardCollectionEditor.Items.Count > 3)
                        card = (CardInCollection)CardCollectionEditor.Items[3];
                    break;
                case Key.D5:
                    if (CardCollectionEditor.Items.Count > 4)
                        card = (CardInCollection)CardCollectionEditor.Items[4];
                    break;
                case Key.Down:
                    if (index < CardCollectionEditor.Items.Count - 1)
                        CardCollectionEditor.SelectedIndex += 1;
                    break;
                case Key.Up:
                    if (index > 0)
                        CardCollectionEditor.SelectedIndex -= 1;
                    break;
            }
            if (card != null)
            {
                UpdateCardsAmount(card, 1);
                e.Handled = true;
            }
        }

        private void UpdateCardsAmount(CardInCollection card, int difference)
        {
            if (Filter.GoldenCards)
            {
                int newValue = card.AmountGolden + difference;
                newValue = newValue.Clamp(0, card.MaxAmountInCollection);
                card.AmountGolden = newValue;
            }
            else
            {
                int newValue = card.AmountNonGolden + difference;
                newValue = newValue.Clamp(0, card.MaxAmountInCollection);
                card.AmountNonGolden = newValue;
            }
        }

        private void CardCollectionEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            int? amount = null;
            if (e.Key == Key.Enter)
            {
                amount = 1;
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                amount = -1;
            }
            if (amount.HasValue)
            {
                CardInCollection card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null || string.IsNullOrEmpty(card.Card.Name))
                    return;

                UpdateCardsAmount(card, amount.Value);
            }
        }

        private void CardCollectionEditor_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, 1);
            }
        }

        private void CardCollectionEditor_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, -1);
            }
        }

        #endregion

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SetCardsManager.Instance.SaveCollection(SetsInfo.Select(s => new BasicSetCollectionInfo
            {
                SetName = s.SetName,
                Cards = s.SetCards.ToList()
            }).ToList());
        }

        private void FlyoutCollection_OnIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (FlyoutCollection.IsOpen)
                TextBoxCollectionFilter.Focus();

            MainWrapPanel.HorizontalAlignment = FlyoutCollection.IsOpen
                ? HorizontalAlignment.Left : HorizontalAlignment.Center;
        }
        
        private void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sets = SetsInfo.Select(s => new BasicSetCollectionInfo
                {
                    SetName = s.SetName,
                    Cards = s.SetCards.ToList()
                }).ToList();

                var clipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "";
                if (string.IsNullOrEmpty(clipboard)) return;
                
                var importedCards = new Dictionary<string, Tuple<int, int>>();
                foreach (var entry in clipboard.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var splitEntry = entry.Split(':');
                    if (splitEntry.Length != 3) continue;

                    var card = Game.GetCardFromId(splitEntry[0]);
                    if (!SetCardsManager.Instance.CollectableSets.Contains(card.Set)) continue;
                    
                    int nonGolden, golden;
                    Int32.TryParse(splitEntry[1], out nonGolden);
                    Int32.TryParse(splitEntry[2], out golden);
                    importedCards.Add(card.Id, new Tuple<int, int>(nonGolden, golden));
                }

                foreach (var card in sets.SelectMany(s => s.Cards))
                {
                    if (importedCards.ContainsKey(card.CardId))
                    {
                        var importedCard = importedCards[card.CardId];
                        card.AmountNonGolden = importedCard.Item1;
                        card.AmountGolden = importedCard.Item2;
                    }
                    else
                    {
                        card.AmountNonGolden = 0;
                        card.AmountGolden = 0;
                    }
                }

                SetCardsManager.Instance.SaveCollection(sets);
                SetsInfo = sets.Select(set => new SetDetailInfoViewModel
                {
                    SetName = set.SetName,
                    SetCards = new TrulyObservableCollection<CardInCollection>(set.Cards.ToList())
                });
            }
            catch (Exception)
            {
                this.ShowMessageAsync("Error", "Could not load collection from clipboard");
            }
        }
        
        private async void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(SetsInfo.SelectMany(s => s.SetCards).Where(c => c.AmountNonGolden + c.AmountGolden > 0).Aggregate("", (s, c) => s + String.Format("{0}:{1}:{2};", c.CardId, c.AmountNonGolden, c.AmountGolden)));
            await this.ShowMessageAsync("", "Copied collection to clipboard");
        }
    }

    internal class CardInCollectionComparer : IComparer
    {
        private const string Neutral = "Neutral";
        public int Compare(object x, object y)
        {
            if (x is CardInCollection && y is CardInCollection)
            {
                CardInCollection cardX = (CardInCollection)x;
                CardInCollection cardY = (CardInCollection)y;
                // workaround to put neutral cards last
                bool xIsNeutral = cardX.CardClass == Neutral;
                bool yIsNeutral = cardY.CardClass == Neutral;
                if (xIsNeutral && !yIsNeutral)
                    return 1;
                if (!xIsNeutral && yIsNeutral)
                    return -1;
                int cardClassCompare = cardX.CardClass.CompareTo(cardY.CardClass);
                if (cardClassCompare != 0)
                    return cardClassCompare;
                int manaCostCompare = cardX.Card.Cost.CompareTo(cardY.Card.Cost);
                if (manaCostCompare != 0)
                    return manaCostCompare;
                int cardTypeCompare = cardX.Card.Type.CompareTo(cardY.Card.Type);
                if (cardTypeCompare != 0)
                    return -cardTypeCompare;
                return cardX.Card.LocalizedName.CompareTo(cardY.Card.LocalizedName);
            }
            else
            {
                return 1;
            }
        }
    }
}

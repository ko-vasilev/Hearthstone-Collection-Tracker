using Hearthstone_Collection_Tracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Collection_Tracker.Controls
{
    /// <summary>
    /// Interaction logic for SetSummary.xaml
    /// </summary>
    public partial class SetSummary : UserControl
    {
        public SetSummary()
        {
            InitializeComponent();
        }

        public event Action<SetDetailInfoViewModel> ManageSetClicked;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (ManageSetClicked != null)
                ManageSetClicked(button.DataContext as SetDetailInfoViewModel);
        }
    }
}

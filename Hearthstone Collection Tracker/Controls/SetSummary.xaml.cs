using Hearthstone_Collection_Tracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            e2.RoutedEvent = UIElement.MouseWheelEvent;

            (sender as DataGrid).RaiseEvent(e2);
        }
    }
}

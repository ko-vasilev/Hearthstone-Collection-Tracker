using Hearthstone_Collection_Tracker.Controls;
using Hearthstone_Collection_Tracker.Internal;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public Window PluginWindow { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            UpdateAccountsComboBox();
        }

        private void UpdateAccountsComboBox()
        {
            ComboboxCurrentAccount.ItemsSource = HearthstoneCollectionTrackerPlugin.Settings.Accounts;
            ComboboxCurrentAccount.Items.Refresh();
            ComboboxCurrentAccount.SelectedValue = HearthstoneCollectionTrackerPlugin.Settings.ActiveAccount;

            ButtonDeleteAccount.IsEnabled = ComboboxCurrentAccount.Items.Count > 1;
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            AddAccountWindow window = new AddAccountWindow();
            window.ExistingAccounts = HearthstoneCollectionTrackerPlugin.Settings.Accounts.Select(acc => acc.AccountName).ToList();
            if (window.ShowDialog() == true)
            {
                HearthstoneCollectionTrackerPlugin.Settings.AddAccount(window.AccountName);
                HearthstoneCollectionTrackerPlugin.Settings.SetActiveAccount(window.AccountName);

                UpdateAccountsComboBox();
            }
        }

        private void ComboboxCurrentAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || e.RemovedItems.Count == 0)
            {
                return;
            }
            // close plugin window
            if (PluginWindow != null && PluginWindow.IsVisible)
            {
                PluginWindow.Close();
            }
            else
            {
                HearthstoneCollectionTrackerPlugin.Settings.SaveCurrentAccount(HearthstoneCollectionTrackerPlugin.Settings.ActiveAccountSetsInfo.ToList());
            }

            string selectedAccountName = (e.AddedItems[0] as AccountSummary).AccountName;
            HearthstoneCollectionTrackerPlugin.Settings.SetActiveAccount(selectedAccountName);
        }

        private async void ButtonDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            string currentAccountName = HearthstoneCollectionTrackerPlugin.Settings.ActiveAccount;
            var messageWindowSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No"
            };
            var result = await this.ShowMessageAsync("Caution",
                string.Format("Do you want to delete account {0}?", currentAccountName),
                MessageDialogStyle.AffirmativeAndNegative,
                messageWindowSettings);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }
            
            // close plugin window
            if (PluginWindow != null && PluginWindow.IsVisible)
            {
                PluginWindow.Close();
            }

            HearthstoneCollectionTrackerPlugin.Settings.DeleteAccount(currentAccountName);
            UpdateAccountsComboBox();
        }
    }
}

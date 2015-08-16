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
using Hearthstone_Collection_Tracker.Internal.Importing;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public Thickness TitleBarMargin
        {
            get { return new Thickness(0, TitlebarHeight, 0, 0); }
        }

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
            window.Owner = this;
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

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            FlyoutImport.IsOpen = true;
        }

        private void TextboxImportDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            int importStartDelay;
            if (int.TryParse(TextboxImportDelay.Text, out importStartDelay))
            {
                if (importStartDelay < 0)
                {
                    TextboxImportDelay.Text = "0";
                }
                else if (importStartDelay > 60)
                {
                    TextboxImportDelay.Text = "60";
                }
            }
            else
            {
                TextboxImportDelay.Text = "0";
            }
        }

        private void TextboxImportDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private async void ButtonImportFromGame_Click(object sender, RoutedEventArgs e)
        {
            const string message = "1) open My Collection in Hearthstone\n2) clear card filters\n3) do not move your mouse or type after clicking \"Import\"";

            var settings = new MetroDialogSettings { AffirmativeButtonText = "Import" };
            var result =
                await
                this.ShowMessageAsync("Import collection from Hearthstone", message, MessageDialogStyle.AffirmativeAndNegative, settings);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            var importObject = new HearthstoneImporter();
            importObject.ImportStepDelay = int.Parse((ComboboxImportSpeed.SelectedItem as ComboBoxItem).Tag.ToString());
            importObject.PasteFromClipboard = CheckboxImportPasteClipboard.IsChecked.HasValue ?
                CheckboxImportPasteClipboard.IsChecked.Value : false;
            importObject.NonGoldenFirst = CheckboxPrioritizeFullCollection.IsChecked.HasValue ?
                CheckboxPrioritizeFullCollection.IsChecked.Value : false;

            try
            {
                var deck = await importObject.Import(TimeSpan.FromSeconds(int.Parse(TextboxImportDelay.Text)));
                // close plugin window
                if (PluginWindow != null && PluginWindow.IsVisible)
                {
                    PluginWindow.Close();
                }
                HearthstoneCollectionTrackerPlugin.Settings.ActiveAccountSetsInfo = deck;
                this.ShowMessageAsync("Import succeed", "Your collection is successfully imported from Hearthstone!");
            }
            catch (ImportingException ex)
            {
                this.ShowMessageAsync("Importing aborted", ex.Message);
            }

            // bring settings window to front
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }
    }
}

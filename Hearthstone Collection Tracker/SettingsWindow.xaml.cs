using Hearthstone_Collection_Tracker.Controls;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.Internal.Importing;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        public PluginSettings Settings { get; set; }

        public SettingsWindow(PluginSettings settings)
        {
            this.Settings = settings;
            InitializeComponent();

            UpdateAccountsComboBox();

            this.DataContext = this;
            var setsOption = SetCardsManager.CollectableSets.Select(s => new KeyValuePair<string, string>(s, s)).ToList();
            setsOption.Insert(0, new KeyValuePair<string, string>("All", null));
            ComboboxImportingSet.ItemsSource = setsOption;
        }

        private void UpdateAccountsComboBox()
        {
            ComboboxCurrentAccount.Items.Clear();
            foreach (var acc in Settings.Accounts)
            {
                ComboboxCurrentAccount.Items.Add(acc);
            }
            ComboboxCurrentAccount.Items.Refresh();
            ComboboxCurrentAccount.SelectedValue = Settings.ActiveAccount;

            ButtonDeleteAccount.IsEnabled = ComboboxCurrentAccount.Items.Count > 1;
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            EditAccountWindow window = new EditAccountWindow();
            window.Owner = this;
            window.ExistingAccounts = Settings.Accounts.Select(acc => acc.AccountName).ToList();
            if (window.ShowDialog() == true)
            {
                Settings.AddAccount(window.AccountName);
                Settings.SetActiveAccount(window.AccountName);

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
                Settings.SaveCurrentAccount(Settings.ActiveAccountSetsInfo.ToList());
            }

            string selectedAccountName = (e.AddedItems[0] as AccountSummary).AccountName;
            Settings.SetActiveAccount(selectedAccountName);
        }

        private async void ButtonDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            string currentAccountName = Settings.ActiveAccount;
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

            Settings.DeleteAccount(currentAccountName);
            UpdateAccountsComboBox();
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            FlyoutImport.IsOpen = true;
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
                var selectedSetToImport = ((KeyValuePair<string, string>)ComboboxImportingSet.SelectedItem).Value;
                var collection = await importObject.Import(selectedSetToImport);
                // close plugin window
                if (PluginWindow != null && PluginWindow.IsVisible)
                {
                    PluginWindow.Close();
                }
                foreach(var set in collection)
                {
                    var existingSet = Settings.ActiveAccountSetsInfo.FirstOrDefault(s => s.SetName == set.SetName);
                    if (existingSet == null)
                    {
                        Settings.ActiveAccountSetsInfo.Add(set);
                    }
                    else
                    {
                        // keep desired amount
                        foreach(var card in set.Cards)
                        {
                            var existingCardInfo = existingSet.Cards.FirstOrDefault(c => c.CardId == card.CardId);
                            if (existingCardInfo != null)
                            {
                                card.DesiredAmount = existingCardInfo.DesiredAmount;
                            }
                        }
                        existingSet.Cards = set.Cards;
                    }
                }
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

        private void ButtonEditAccount_Click(object sender, RoutedEventArgs e)
        {
            EditAccountWindow window = new EditAccountWindow();
            window.Owner = this;
            window.ExistingAccounts = Settings.Accounts
                                              .Where(ac => ac.AccountName != Settings.ActiveAccount)
                                              .Select(acc => acc.AccountName)
                                              .ToList();
            window.AccountName = Settings.ActiveAccount;
            if (window.ShowDialog() == true)
            {
                Settings.RenameCurrentAccount(window.AccountName);

                UpdateAccountsComboBox();
            }
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            FlyoutUpdate.IsOpen = true;
        }

        private async void ButtonUpdateFromGame_Click(object sender, RoutedEventArgs e)
        {
            const string message = "1) open My Collection in Hearthstone\n2) clear card filters\n3) do not move your mouse or type after clicking \"Update\"";

            var settings = new MetroDialogSettings { AffirmativeButtonText = "Update" };
            var result =
                await
                this.ShowMessageAsync("Update collection from Hearthstone", message, MessageDialogStyle.AffirmativeAndNegative, settings);

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
                var currentCollection = Settings.ActiveAccountSetsInfo.ToList();
                var collection = await importObject.Update(currentCollection);
                // close plugin window
                if (PluginWindow != null && PluginWindow.IsVisible)
                {
                    PluginWindow.Close();
                }
                foreach (var set in collection)
                {
                    var existingSet = Settings.ActiveAccountSetsInfo.FirstOrDefault(s => s.SetName == set.SetName);
                    if (existingSet == null)
                    {
                        Settings.ActiveAccountSetsInfo.Add(set);
                    }
                    else
                    {
                        existingSet.Cards = set.Cards;
                    }
                }
                await this.ShowMessageAsync("Update succeed", "Your collection has been successfully updated from Hearthstone!");
            }
            catch (ImportingException ex)
            {
                await this.ShowMessageAsync("Update aborted", ex.Message);
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

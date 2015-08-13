using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Hearthstone_Collection_Tracker.Controls
{
    /// <summary>
    /// Interaction logic for AddAccountWindow.xaml
    /// </summary>
    public partial class AddAccountWindow : MetroWindow, IDataErrorInfo
    {
        public AddAccountWindow()
        {
            ExistingAccounts = new List<string>();

            InitializeComponent();

            this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));

            TextboxName.Focus();
        }

        public List<string> ExistingAccounts { get; set; }

        public string AccountName { get; set; }
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #region Validation
        private int errorCount;
        private void OnErrorEvent(object sender, RoutedEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null)
                throw new Exception("Unexpected event args");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                    {
                        errorCount++; break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        errorCount--; break;
                    }
                default:
                    {
                        throw new Exception("Unknown action");
                    }
            }
            AddButton.IsEnabled = errorCount == 0;
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "AccountName")
                {
                    if (ExistingAccounts != null && ExistingAccounts.Contains(AccountName))
                    {
                        result = "Account with this name already exists";
                    }
                }
                return result;
            }
        }
        #endregion
    }
}

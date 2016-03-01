using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Collection_Tracker.Internal
{
    [Serializable]
    public class PluginSettings
    {
        public ModuleVersion CurrentVersion { get; set; }

        public string ActiveAccount { get; set; }

        public List<AccountSummary> Accounts { get; set; }

        public double CollectionWindowWidth { get; set; }

        public double CollectionWindowHeight { get; set; }

        public bool DefaultShowAllCards { get; set; }

        public bool NotifyNewDeckMissingCards { get; set; }

        public bool EnableDesiredCardsFeature { get; set; }

        [NonSerialized]
        [XmlIgnore]
        private IList<BasicSetCollectionInfo> _activeAccountSetsInfo;

        [XmlIgnore]
        public IList<BasicSetCollectionInfo> ActiveAccountSetsInfo
        {
            get { return _activeAccountSetsInfo; }
            set { _activeAccountSetsInfo = value; }
        }

        private const string STORAGE_FILE_NAME = "config.xml";

        public void SetActiveAccount(string accountName, bool forceReload = false)
        {
            if (accountName == ActiveAccount && !forceReload)
            {
                return;
            }
            var activeAccount = Accounts.FirstOrDefault(ac => ac.AccountName == accountName);
            if (activeAccount == null)
            {
                Log.WriteLine("Cannot set active account " + accountName + " because it does not exist", LogType.Debug, "CollectionTracker.PluginSettings");
                return;
            }

            if (File.Exists(activeAccount.FileStoragePath))
            {
                _activeAccountSetsInfo = SetCardsManager.LoadSetsInfo(activeAccount.FileStoragePath);
            }
            else
            {
                _activeAccountSetsInfo = SetCardsManager.CreateEmptyCollection();
            }
            ActiveAccount = accountName;
        }

        public void AddAccount(string accountName)
        {
            var existingAccount = Accounts.FirstOrDefault(acc => acc.AccountName == accountName);
            if (existingAccount != null)
            {
                Log.WriteLine("Account already exists: " + accountName, LogType.Debug, "CollectionTracker.PluginSettings");
                SetActiveAccount(accountName);
                return;
            }

            string accountFilePath = Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_" +  accountName.GetValidFileName());
            if (File.Exists(accountFilePath + ".xml"))
            {
                string basicFilePath = accountFilePath;
                string pathWithExtension;
                int i = 0;
                do
                {
                    ++i;
                    accountFilePath = basicFilePath + i;
                    pathWithExtension = accountFilePath + ".xml";
                } while (File.Exists(pathWithExtension) || Accounts.Any(acc => acc.FileStoragePath == pathWithExtension));
            }

            AccountSummary newAccount = new AccountSummary()
            {
                AccountName = accountName,
                FileStoragePath = accountFilePath + ".xml"
            };
            Accounts.Add(newAccount);
        }

        public void DeleteAccount(string accountName)
        {
            var account = Accounts.FirstOrDefault(acc => acc.AccountName == accountName);
            if (account == null)
            {
                return;
            }

            Accounts.Remove(account);

            if (ActiveAccount == accountName && Accounts.Any())
            {
                SetActiveAccount(Accounts.First().AccountName);
            }
            if (File.Exists(account.FileStoragePath))
            {
                File.Delete(account.FileStoragePath);
            }
        }

        public void SaveCurrentAccount(List<BasicSetCollectionInfo> setsInfo)
        {
            var activeAccount = Accounts.First(acc => acc.AccountName == ActiveAccount);
            SetCardsManager.SaveCollection(setsInfo, activeAccount.FileStoragePath);
        }

        public void SaveCurrentAccount()
        {
            SaveCurrentAccount(ActiveAccountSetsInfo.ToList());
        }

        public static PluginSettings LoadSettings(string dataDir)
        {
            string settingsFilePath = Path.Combine(dataDir, STORAGE_FILE_NAME);
            PluginSettings settings;
            if (File.Exists(settingsFilePath))
            {
                settings = XmlManager<PluginSettings>.Load(settingsFilePath);
            }
            else
            {
                string collectionFilePath = Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_Default.xml");
                settings = new PluginSettings()
                {
                    CurrentVersion = new ModuleVersion(HearthstoneCollectionTrackerPlugin.PluginVersion),
                    Accounts = new List<AccountSummary>()
                    {
                        new AccountSummary()
                        {
                            AccountName = "Default",
                            FileStoragePath = collectionFilePath
                        }
                    },
                    ActiveAccount = "Default",
                    CollectionWindowWidth = 395,
                    CollectionWindowHeight = 560,
                    DefaultShowAllCards = false
                };
            }

            settings.SetActiveAccount(settings.ActiveAccount, true);

            return settings;
        }

        public void RenameCurrentAccount(string newName)
        {
            var account = Accounts.First(a => a.AccountName == ActiveAccount);
            account.AccountName = newName;
            ActiveAccount = newName;
        }

        public void SaveSettings(string dataDir)
        {
            string settingsFilePath = Path.Combine(dataDir, STORAGE_FILE_NAME);
            XmlManager<PluginSettings>.Save(settingsFilePath, this);
        }
    }

    [Serializable]
    public class AccountSummary
    {
        public string AccountName { get; set; }

        public string FileStoragePath { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is AccountSummary) || obj == null)
                return false;
            var o = obj as AccountSummary;
            return AccountName.Equals(o.AccountName) && FileStoragePath.Equals(o.FileStoragePath);
        }
    }
}

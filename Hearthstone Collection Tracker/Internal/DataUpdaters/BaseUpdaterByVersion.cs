using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public abstract class BaseUpdaterByVersion : IDataUpdater
    {
        public abstract Version Version { get; }

        protected string ConfigFilePath
        {
            get { return Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "config.xml"); }
        }

        public bool RequiresUpdate
        {
            get
            {
                var configFilePath = ConfigFilePath;
                if (!Directory.Exists(HearthstoneCollectionTrackerPlugin.PluginDataDir) || !File.Exists(configFilePath))
                {
                    return false;
                }

                try
                {
                    var serializer = new XmlSerializer(typeof(ModuleVersion));
                    var settingsEl = new XElement(SettingsNode);
                    settingsEl.Name = "ModuleVersion";
                    var currentVersion = (ModuleVersion)serializer.Deserialize(settingsEl.CreateReader());

                    return currentVersion < new ModuleVersion(Version);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private XDocument _settingsDocument;

        private XDocument SettingsDocument
        {
            get
            {
                if (_settingsDocument == null)
                {
                    _settingsDocument = XDocument.Load(ConfigFilePath);
                }
                return _settingsDocument;
            }
        }

        private XElement SettingsNode
        {
            get
            {
                return SettingsDocument.Descendants("CurrentVersion").First();
            }
        }

        public void PerformUpdate()
        {
            SettingsNode.Descendants("Major").First().SetValue(Version.Major);
            SettingsNode.Descendants("Minor").First().SetValue(Version.Minor);
            SettingsNode.Descendants("Build").First().SetValue(Version.Build);
            SettingsNode.Descendants("Revision").First().SetValue(Version.Revision);
            SettingsDocument.Save(ConfigFilePath);
        }
    }
}

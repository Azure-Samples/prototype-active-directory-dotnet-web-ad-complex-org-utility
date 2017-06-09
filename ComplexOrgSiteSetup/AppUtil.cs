using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace ComplexOrgSiteSetup
{
    public static class AppUtil
    {
        public static bool ModifyRemoteConfig(string configPath, Dictionary<string, string> updates)
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configPath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                var settings = (config.SectionGroups["userSettings"].Sections[0] as ClientSettingsSection).Settings;
                SettingElement el;

                foreach (var item in updates)
                {
                    el = settings.Get(item.Key);
                    el.Value.ValueXml.InnerText = item.Value;
                }

                config.Save(ConfigurationSaveMode.Minimal, true);
                return true;
            }
            catch (Exception ex)
            {
                var msg = Utils.GetFormattedException(ex);
                Utils.AddLogEntry(Setup.LogSource, msg, EventLogEntryType.Error);
                return false;
            }
        }
        public static bool ModifyJsonConfig(string configPath, Dictionary<string, string> updates)
        {
            try
            {
                var config = JsonConvert.SerializeObject(updates);
                System.IO.File.WriteAllText(configPath, config);
                return true;
            }
            catch (Exception ex)
            {
                var msg = Utils.GetFormattedException(ex);
                Utils.AddLogEntry(Setup.LogSource, msg, EventLogEntryType.Error);
                return false;
            }
        }
        public static bool ModifyAppConfig(string configPath, Dictionary<string, string> updates)
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configPath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                var settings = (config.AppSettings as AppSettingsSection).Settings;

                KeyValueConfigurationElement el;

                foreach (var item in updates)
                {
                    el = settings[item.Key];
                    if (el == null) continue;
                    el.Value = item.Value;
                }

                config.Save(ConfigurationSaveMode.Minimal, true);
                return true;
            }
            catch (Exception ex)
            {
                var msg = Utils.GetFormattedException(ex);
                Utils.AddLogEntry(Setup.LogSource, msg, EventLogEntryType.Error);
                return false;
            }
        }
    }
}

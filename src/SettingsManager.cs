namespace XBOneHDDMaker {
    using System;
    using System.Collections.Generic;
    using Microsoft.Win32;

    internal class SettingsManager {
        private readonly Dictionary<string, bool> _booleanSettings = new Dictionary<string, bool>();
        private readonly RegistryKey _internalKey;
        private readonly Dictionary<string, string> _stringSettings = new Dictionary<string, string>();
        private readonly Dictionary<string, ulong> _ulongSettings = new Dictionary<string, ulong>();

        private SettingsManager(string appName) {
            _internalKey = Registry.CurrentUser.OpenSubKey(string.Format("Software\\SwizzySoft\\{0}", appName));
            if(_internalKey == null)
                throw new Exception("Cannot access the registry key!");
        }

        public string GetStringSetting(string setting, string defaultvalue = "") {
            if(_stringSettings.ContainsKey(setting))
                return _stringSettings[setting];
            var ret = _internalKey.GetValue(setting, defaultvalue) as string;
            _stringSettings.Add(setting, ret);
            return ret;
        }

        public ulong GetulongSetting(string setting, ulong defaultvalue = 0) {
            if(_ulongSettings.ContainsKey(setting))
                return _ulongSettings[setting];
            var ret = (ulong) _internalKey.GetValue(setting, defaultvalue);
            _ulongSettings.Add(setting, ret);
            return ret;
        }

        public bool GetBoolSetting(string setting, bool defaultvalue = false) {
            if(_booleanSettings.ContainsKey(setting))
                return _booleanSettings[setting];
            var ret = (int) _internalKey.GetValue(setting, defaultvalue ? 1 : 0);
            _booleanSettings.Add(setting, ret != 0);
            return ret != 0;
        }
    }
}
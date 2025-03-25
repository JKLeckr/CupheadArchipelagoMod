/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using CupheadArchipelago.AP;
using Newtonsoft.Json;

namespace CupheadArchipelago {
    internal class SaveData {
        private static bool initted = false;

        private const string AP_SAVE_FILE_KEY_SUFFIX = "_apdata";
        internal static readonly string SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");

        internal static readonly string[] AP_SAVE_FILE_KEYS = new string[3];

        private static int attempts = 0;

        internal static void Init(string saveKeyName) {
            for (int i=0;i<3;i++) {
                AP_SAVE_FILE_KEYS[i] = $"{saveKeyName}{i}{AP_SAVE_FILE_KEY_SUFFIX}";
            }
            initted = true;
        }

        internal static void LoadAPData(LoadCloudDataHandler handler) {
            Load(AP_SAVE_FILE_KEYS, handler);
        }
        private static void Load(string[] keys, LoadCloudDataHandler handler) {
            OnlineManager.Instance.Interface.LoadCloudData(keys, handler);
        }

        internal static void SaveAPData(int slot, string data) {
            Save(AP_SAVE_FILE_KEYS[slot], data, new SaveCloudDataHandler(OnAPDataSave));
        }
        private static void Save(string key, string data, SaveCloudDataHandler handler) {
            Dictionary<string, string> datadict = [];
            datadict[key] = data;
            OnlineManager.Instance.Interface.SaveCloudData(datadict, handler);
        }

        private static void OnAPDataSave(bool success) {
            if (success)
                Logging.Log("[SaveData] Save Success");
            else
                Logging.LogError("[SaveData] Save Failure");
        }

        public static bool IsInitialized() => initted;
    }
}

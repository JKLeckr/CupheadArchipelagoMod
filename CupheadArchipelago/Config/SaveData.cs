/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace CupheadArchipelago.Config {
    public class SaveData {
        private static bool initted = false;

        private const string AP_SAVE_FILE_KEY_SUFFIX = "_apdata";
        
        internal static readonly string DEFAULT_SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");
        internal static readonly string[] AP_SAVE_FILE_KEYS = new string[3];

        internal static string APSavePath { get; private set; } = "";

        public static void Init(string saveKeyName) => Init(saveKeyName, DEFAULT_SAVE_PATH, false);
        public static void Init(string saveKeyName, string savePath) => Init(saveKeyName, savePath, false);
        public static void Init(string saveKeyName, string savePath, bool iKnowWhatImDoing) {
            if (initted && !iKnowWhatImDoing) Logging.LogWarning("Reinitializing SaveData...");
            for (int i=0;i<3;i++) {
                AP_SAVE_FILE_KEYS[i] = $"{saveKeyName}{i}{AP_SAVE_FILE_KEY_SUFFIX}";
            }
            APSavePath = savePath;
            initted = true;
        }

        public static bool IsInitialized() => initted;
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace CupheadArchipelago {
    internal class SaveData {
        private static bool initted = false;

        private const string AP_SAVE_FILE_KEY_SUFFIX = "_apdata";
        internal static readonly string AP_SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");

        internal static readonly string[] AP_SAVE_FILE_KEYS = new string[3];

        internal static void Init(string saveKeyName) {
            for (int i=0;i<3;i++) {
                AP_SAVE_FILE_KEYS[i] = $"{saveKeyName}{i}{AP_SAVE_FILE_KEY_SUFFIX}";
            }
            initted = true;
        }

        public static bool IsInitialized() => initted;
    }
}

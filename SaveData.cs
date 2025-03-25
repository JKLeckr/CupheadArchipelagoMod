/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using CupheadArchipelago.AP;

namespace CupheadArchipelago {
    internal class SaveData {
        private static bool initted = false;

        internal static void Init(string saveKeyName) {
            APData.Init(saveKeyName);
            initted = true;
        }
    }
}

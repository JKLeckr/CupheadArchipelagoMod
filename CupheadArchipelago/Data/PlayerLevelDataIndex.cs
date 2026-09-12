/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Data {
    internal class LevelPlayerDataIndex(int slot) {
        public static LevelPlayerDataIndex Current { get; protected set; }

        private readonly Dictionary<Levels, PlayerData.PlayerLevelDataObject> pldIndex = [];
        private readonly int slot = slot;

        public static void Init() {
            Current = new(PlayerData.CurrentSaveFileIndex);
        }

        public PlayerData.PlayerLevelDataObject GetLevelData(Levels level) {
            if (!pldIndex.ContainsKey(level) || pldIndex[level] == null) {
                pldIndex[level] = PlayerData.GetDataForSlot(slot).GetLevelData(level);
            }
            return pldIndex[level];
        }
    }
}

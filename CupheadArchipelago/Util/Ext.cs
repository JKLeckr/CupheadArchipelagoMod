/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Mapping;

namespace CupheadArchipelago.Util {
    public static class Ext {
        public static Levels[] LMapped(this Levels[] levels) {
            if (!LevelMap.IsInitted()) {
                Logging.LogWarning("LevelMap is not initted! Returning unmapped levels.");
                return levels;
            }
            Levels[] res = new Levels[levels.Length];
            for (int i = 0; i < res.Length; i++) {
                res[i] = LevelMap.GetMappedLevel(levels[i]);
            }
            return res;
        }

        public static bool CheckAnyLevelComplete(this Levels[] levels) {
            PlayerData data = PlayerData.Data;
            foreach (Levels l in levels) {
                if (data.CheckLevelCompleted(l))
                    return true;
            }
            return false;
        }
        public static bool CheckLevelsComplete(this Levels[] levels) {
            if (levels.Length < 1) return false;
            return PlayerData.Data.CheckLevelsCompleted(levels);
        }
    }
}

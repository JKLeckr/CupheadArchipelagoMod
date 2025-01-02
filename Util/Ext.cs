/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Util {
    public class Ext {
        public static bool CheckLevelComplete(Levels level) {
            return PlayerData.Data.CheckLevelCompleted(level);
        }
        public static bool CheckAnyLevelComplete(Levels[] levels) {
            PlayerData data = PlayerData.Data;
            foreach (Levels l in levels) {
                if (data.CheckLevelCompleted(l))
                    return true;
            }
            return false;
        }
        public static bool CheckLevelsComplete(Levels[] levels) {
            if (levels.Length<1) return false;
            return PlayerData.Data.CheckLevelsCompleted(levels);
        }
    } 
}

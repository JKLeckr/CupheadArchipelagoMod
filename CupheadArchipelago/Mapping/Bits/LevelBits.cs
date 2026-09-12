/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Mapping.Bits {
    public class LevelBits {
        private static readonly Dictionary<Levels, int> levelBossBitIds = new() {
            {Levels.Veggies, 0},
            {Levels.Slime, 1},
            {Levels.Frogs, 2},
            {Levels.Flower, 3},
            {Levels.Baroness, 4},
            {Levels.Clown, 5},
            {Levels.Dragon, 6},
            {Levels.Bee, 7},
            {Levels.Pirate, 8},
            {Levels.Mouse, 9},
            {Levels.SallyStagePlay, 10},
            {Levels.Train, 11},
            {Levels.FlyingBlimp, 12},
            {Levels.FlyingGenie, 13},
            {Levels.FlyingBird, 14},
            {Levels.FlyingMermaid, 15},
            {Levels.Robot, 16},
            {Levels.DicePalaceMain, 17},
            {Levels.Devil, 18},

            {Levels.OldMan, 19},
            {Levels.RumRunners, 20},
            {Levels.SnowCult, 21},
            {Levels.Airplane, 22},
            {Levels.FlyingCowboy, 23},
            {Levels.Saltbaker, 24},

            {Levels.Graveyard, 25},
        };

        private static readonly Dictionary<Levels, int> levelBossDicePalaceBitIds = new() {
            {Levels.DicePalaceBooze, 0},
            {Levels.DicePalaceChips, 1},
            {Levels.DicePalaceCigar, 2},
            {Levels.DicePalaceDomino, 3},
            {Levels.DicePalaceRabbit, 4},
            {Levels.DicePalaceFlyingHorse, 5},
            {Levels.DicePalaceRoulette, 6},
            {Levels.DicePalaceEightBall, 7},
            {Levels.DicePalaceFlyingMemory, 8}
        };

        private static readonly Dictionary<Levels, int> levelBossDlcChessCastleBitIds = new() {
            {Levels.ChessPawn, 0},
            {Levels.ChessKnight, 1},
            {Levels.ChessBishop, 2},
            {Levels.ChessRook, 3},
            {Levels.ChessQueen, 4},
            {Levels.ChessCastle, 5},
        };

        private static readonly Dictionary<Levels, int> levelRunGunBitIds = new() {
            {Levels.Platforming_Level_1_1, 0},
            {Levels.Platforming_Level_1_2, 1},
            {Levels.Platforming_Level_2_1, 2},
            {Levels.Platforming_Level_2_2, 3},
            {Levels.Platforming_Level_3_1, 4},
            {Levels.Platforming_Level_3_2, 5},
        };

        public static bool IsBitableBossLevel(Levels level) {
            return levelBossBitIds.ContainsKey(level);
        }

        public static int GetBossLevelBit(Levels level) {
            if (!levelBossBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "' is not a valid Boss level.");
            }
            return 1 << levelBossBitIds[level];
        }

        public static int GetBossLevelBitId(Levels level) {
            if (!levelBossBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "' is not a valid Boss level.");
            }
            return levelBossBitIds[level];
        }

        public static Levels[] GetBittableBossLevels() {
            return [.. levelBossBitIds.Keys];
        }

        public static bool IsBitableRunGunLevel(Levels level) {
            return levelRunGunBitIds.ContainsKey(level);
        }

        public static int GetRunGunLevelBit(Levels level) {
            if (!levelRunGunBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid RunGun level.");
            }
            return 1 << levelRunGunBitIds[level];
        }

        public static int GetRunGunLevelBitId(Levels level) {
            if (!levelRunGunBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid RunGun level.");
            }
            return levelRunGunBitIds[level];
        }

        public static Levels[] GetBittableRunGunLevels() {
            return [.. levelRunGunBitIds.Keys];
        }

        public static bool IsBitableDicePalaceBossLevel(Levels level) {
            return levelBossDicePalaceBitIds.ContainsKey(level);
        }

        public static int GetDicePalaceBossLevelBit(Levels level) {
            if (!levelBossDicePalaceBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DicePalace level.");
            }
            return 1 << levelBossDicePalaceBitIds[level];
        }

        public static int GetDicePalaceBossLevelBitId(Levels level) {
            if (!levelBossDicePalaceBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DicePalace level.");
            }
            return levelBossDicePalaceBitIds[level];
        }

        public static Levels[] GetBittableDicePalaceBossLevels() {
            return [.. levelBossDicePalaceBitIds.Keys];
        }

        public static bool IsBitableDlcChessBossLevel(Levels level) {
            return levelBossDlcChessCastleBitIds.ContainsKey(level);
        }

        public static int GetDlcChessBossLevelBit(Levels level) {
            if (!levelBossDlcChessCastleBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DlcChess level.");
            }
            return 1 << levelBossDlcChessCastleBitIds[level];
        }

        public static int GetDlcChessBossLevelBitId(Levels level) {
            if (!levelBossDlcChessCastleBitIds.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DlcChess level.");
            }
            return levelBossDlcChessCastleBitIds[level];
        }

        public static Levels[] GetBittableDlcChessBossLevels() {
            return [.. levelBossDlcChessCastleBitIds.Keys];
        }
    }
}

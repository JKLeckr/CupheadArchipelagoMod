/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Mapping.Bits {
    public class LevelBits {
        private static readonly Dictionary<Levels, int> levelBossBits = new() {
            {Levels.Veggies, 1 << 0},
            {Levels.Slime, 1 << 1},
            {Levels.Frogs, 1 << 2},
            {Levels.Flower, 1 << 3},
            {Levels.Baroness, 1 << 4},
            {Levels.Clown, 1 << 5},
            {Levels.Dragon, 1 << 6},
            {Levels.Bee, 1 << 7},
            {Levels.Pirate, 1 << 8},
            {Levels.Mouse, 1 << 9},
            {Levels.SallyStagePlay, 1 << 10},
            {Levels.Train, 1 << 11},
            {Levels.FlyingBlimp, 1 << 12},
            {Levels.FlyingGenie, 1 << 13},
            {Levels.FlyingBird, 1 << 14},
            {Levels.FlyingMermaid, 1 << 15},
            {Levels.Robot, 1 << 16},
            {Levels.DicePalaceMain, 1 << 17},
            {Levels.Devil, 1 << 18},

            {Levels.OldMan, 1 << 19},
            {Levels.RumRunners, 1 << 20},
            {Levels.SnowCult, 1 << 21},
            {Levels.Airplane, 1 << 22},
            {Levels.FlyingCowboy, 1 << 23},
            {Levels.Saltbaker, 1 << 24},

            {Levels.Graveyard, 1 << 25},
        };

        private static readonly Dictionary<Levels, int> levelBossDicePalaceBits = new() {
            {Levels.DicePalaceBooze, 1 << 0},
            {Levels.DicePalaceChips, 1 << 1},
            {Levels.DicePalaceCigar, 1 << 2},
            {Levels.DicePalaceDomino, 1 << 3},
            {Levels.DicePalaceRabbit, 1 << 4},
            {Levels.DicePalaceFlyingHorse, 1 << 5},
            {Levels.DicePalaceRoulette, 1 << 6},
            {Levels.DicePalaceEightBall, 1 << 7},
            {Levels.DicePalaceFlyingMemory, 1 << 8}
        };

        private static readonly Dictionary<Levels, int> levelBossDlcChessCastleBits = new() {
            {Levels.ChessPawn, 1 << 0},
            {Levels.ChessKnight, 1 << 1},
            {Levels.ChessBishop, 1 << 2},
            {Levels.ChessRook, 1 << 3},
            {Levels.ChessQueen, 1 << 4},
            {Levels.ChessCastle, 1 << 5},
        };

        private static readonly Dictionary<Levels, int> levelRunGunBits = new() {
            {Levels.Platforming_Level_1_1, 1 << 0},
            {Levels.Platforming_Level_1_2, 1 << 1},
            {Levels.Platforming_Level_2_1, 1 << 2},
            {Levels.Platforming_Level_2_2, 1 << 3},
            {Levels.Platforming_Level_3_1, 1 << 4},
            {Levels.Platforming_Level_3_2, 1 << 5},
        };

        public static int GetBossLevelBit(Levels level) {
            if (!levelBossBits.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid Boss level.");
            }
            return levelBossBits[level];
        }

        public static Levels[] GetBittableBossLevels() {
            return [.. levelBossBits.Keys];
        }

        public static int GetRunGunLevelBit(Levels level) {
            if (!levelRunGunBits.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid RunGun level.");
            }
            return levelRunGunBits[level];
        }

        public static Levels[] GetBittableRunGunLevels() {
            return [.. levelRunGunBits.Keys];
        }

        public static int GetDicePalaceBossLevelBit(Levels level) {
            if (!levelBossDicePalaceBits.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DicePalace level.");
            }
            return levelBossDicePalaceBits[level];
        }

        public static Levels[] GetBittableDicePalaceBossLevels() {
            return [.. levelBossDicePalaceBits.Keys];
        }

        public static int GetDlcChessBossLevelBit(Levels level) {
            if (!levelBossDlcChessCastleBits.ContainsKey(level)) {
                throw new KeyNotFoundException("'" + level + "'" + " is not a valid DlcChess level.");
            }
            return levelBossDlcChessCastleBits[level];
        }

        public static Levels[] GetBittableDlcChessBossLevels() {
            return [.. levelBossDlcChessCastleBits.Keys];
        }
    }
}

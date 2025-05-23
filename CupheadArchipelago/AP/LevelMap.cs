/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class LevelMap {
        private static readonly Dictionary<long, Levels> levelMap = new() {
            {0, Levels.Veggies},
            {1, Levels.Slime},
            {2, Levels.Frogs},
            {3, Levels.Flower},
            {4, Levels.Baroness},
            {5, Levels.Clown},
            {6, Levels.Dragon},
            {7, Levels.Bee},
            {8, Levels.Pirate},
            {9, Levels.Mouse},
            {10, Levels.SallyStagePlay},
            {11, Levels.Train},
            {12, Levels.FlyingBlimp},
            {13, Levels.FlyingGenie},
            {14, Levels.FlyingBird},
            {15, Levels.FlyingMermaid},
            {16, Levels.Robot},
            //{17, Levels.DicePalaceMain},
            //{18, Levels.Devil},
            //{19, Levels.DicePalaceBooze},
            //{20, Levels.DicePalaceChips},
            //{21, Levels.DicePalaceCigar},
            //{22, Levels.DicePalaceDomino},
            //{23, Levels.DicePalaceRabbit},
            //{24, Levels.DicePalaceFlyingHorse},
            //{25, Levels.DicePalaceRoulette},
            //{26, Levels.DicePalaceEightBall},
            //{27, Levels.DicePalaceFlyingMemory},
            {28, Levels.Platforming_Level_1_1},
            {29, Levels.Platforming_Level_1_2},
            {30, Levels.Platforming_Level_2_1},
            {31, Levels.Platforming_Level_2_2},
            {32, Levels.Platforming_Level_3_1},
            {33, Levels.Platforming_Level_3_2},
            {100, Levels.OldMan},
            {101, Levels.RumRunners},
            {102, Levels.SnowCult},
            {103, Levels.Airplane},
            {104, Levels.FlyingCowboy},
            //{105, Levels.Saltbaker},
            //{106, Levels.Graveyard},
            //{107, Levels.ChessPawn},
            //{108, Levels.ChessKnight},
            //{109, Levels.ChessBishop},
            //{110, Levels.ChessRook},
            //{111, Levels.ChessQueen},
            //{112, Levels.ChessCastle},
        };
        private static readonly Dictionary<Levels, long> levelIdMap = [];

        private static readonly HashSet<Levels> bossLevels;
        private static readonly HashSet<Levels> rungunLevels;

        private static LevelMap instance = null;

        static LevelMap() {
            foreach (long key in levelMap.Keys) {
                levelIdMap.Add(levelMap[key], key);
            }
            bossLevels = [
                .. Level.world1BossLevels,
                .. Level.world2BossLevels,
                .. Level.world3BossLevels,
                .. Level.worldDLCBossLevels
            ];
            rungunLevels = [.. Level.platformingLevels];
        }

        internal static void Init(LevelMap map) {
            instance = map;
        }

        public static bool IsInitted() => instance != null;

        public static bool LevelIdExists(long id) => levelMap.ContainsKey(id);
        public static bool LevelExists(Levels level) => levelIdMap.ContainsKey(level);
        public static long GetLevelId(Levels level) => levelIdMap[level];
        public static bool LevelIsBoss(Levels level) => bossLevels.Contains(level) && LevelExists(level);
        public static bool LevelIsRungun(Levels level) => rungunLevels.Contains(level) && LevelExists(level);

        public static Levels GetMappedLevel(Levels orig) {
            if (IsInitted()) return instance.MapLevel(orig);
            else {
                Logging.LogError("[LevelMap] Not initted! Cannot Map!");
                return orig;
            }
        }

        private readonly Dictionary<Levels, Levels> shuffleMap;

        public LevelMap(IDictionary<long, long> map) {
            shuffleMap = [];
            foreach (long lid in levelMap.Keys) {
                Levels level = levelMap[lid];
                if (map.ContainsKey(lid)) {
                    // TODO: eventually support more combinations
                    Levels mappedLevel = levelMap[map[lid]];
                    if ((LevelIsBoss(level) && LevelIsBoss(mappedLevel)) ||
                        (LevelIsRungun(level) && LevelIsRungun(mappedLevel))
                    ) {
                        shuffleMap.Add(level, mappedLevel);
                    }
                    else {
                        Logging.LogError($"[LevelMap] Invalid map combination: \"{level} -> {mappedLevel}\" Type mismatch!");
                        throw new ArgumentException("Levels must be mapped to the same type!");
                    }
                }
                else {
                    shuffleMap.Add(levelMap[lid], levelMap[lid]);
                }
            }
        }

        public Levels MapLevel(Levels orig) {
            if (shuffleMap.ContainsKey(orig))
                return shuffleMap[orig];
            else {
                Logging.Log($"[GetMappedLevel] Level \"{orig}\" is not mapped. Returning original level.");
                return orig;
            }
        }
    }
}

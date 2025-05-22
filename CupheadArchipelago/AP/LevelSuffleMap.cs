/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class LevelShuffleMap {
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
            {17, Levels.DicePalaceMain},
            {18, Levels.Devil},
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
            {105, Levels.Saltbaker},
            {106, Levels.Graveyard},
            {107, Levels.ChessPawn},
            {108, Levels.ChessKnight},
            {109, Levels.ChessBishop},
            {110, Levels.ChessRook},
            {111, Levels.ChessQueen},
            {112, Levels.ChessCastle},
        };
        private static readonly Dictionary<Levels, long> levelIdMap = [];

        static LevelShuffleMap() {
            foreach (long key in levelMap.Keys) {
                levelIdMap.Add(levelMap[key], key);
            }
        }

        private readonly Dictionary<Levels, Levels> shuffleMap;

        public LevelShuffleMap(IDictionary<long, long> map) {
            shuffleMap = [];
            foreach (long lid in levelMap.Keys) {
                if (map.ContainsKey(lid)) {
                    shuffleMap.Add(levelMap[lid], levelMap[map[lid]]);
                } else {
                    shuffleMap.Add(levelMap[lid], levelMap[lid]);
                }
            }
        }

        public Levels GetShuffledLevel(Levels orig) => shuffleMap[orig];
        public long GetLevelShuffleId(Levels level) => levelIdMap[level];
    }
}

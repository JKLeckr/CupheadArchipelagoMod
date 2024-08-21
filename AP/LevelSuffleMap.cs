/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class LevelShuffleMap {
        private static Dictionary<long, Level> levelMap = new() {};

        private Dictionary<Level, Level> shuffleMap;

        public LevelShuffleMap(IDictionary<long, long> map) {
            shuffleMap = new();
            foreach (KeyValuePair<long, long> items in map) {
                shuffleMap.Add(levelMap[items.Key], levelMap[items.Value]);
            }
        }

        public Level GetShuffledLevel(Level orig) => shuffleMap[orig];
    }
}

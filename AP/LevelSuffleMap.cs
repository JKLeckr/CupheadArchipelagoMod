/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class LevelShuffleMap {
        private Dictionary<string, string> shuffleMap;

        public LevelShuffleMap(IDictionary<string, string> map) {
            this.shuffleMap = new(map);
        }

        public string GetShuffledLevel(string orig) => shuffleMap[orig];
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class CoinIdMap {
        private static readonly Dictionary<string,APLocation> idToLoc = new() {
            {"a53bbd1a-734e-4e60-ada8-d11c62eabcec", APLocation.level_tutorial_coin},
            {"0000", APLocation.coin_isle1_secret},
            {"0001", APLocation.coin_isle2_secret},
            {"0002", APLocation.coin_isle3_secret},
            {"0003", APLocation.coin_isleh_secret},
            {"0004", APLocation.dlc_coin_isle4_secret},
        };

        public static bool CoinIDExists(string coinId) => idToLoc.ContainsKey(coinId);
        public static long GetAPLocationId(string coinId) => idToLoc[coinId];
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class CoinIdMap {
        private static readonly Dictionary<string,APLocation> idToLoc = new() {
            {"a53bbd1a-734e-4e60-ada8-d11c62eabcec", APLocation.level_tutorial_coin},
            {"675028e9-b9d6-4d31-8536-8ff8e98e2ddf", APLocation.coin_isle1_secret},
            {"1782e7b4-2edf-45c0-b312-3083397307bf", APLocation.coin_isle2_secret},
            {"e312336e-010f-4ea4-975b-922aca63629e", APLocation.coin_isle3_secret},
            {"43dfad5b-65dc-42f1-9ab3-25e0174f4ee8", APLocation.coin_isleh_secret},
        };

        public static bool CoinIDExists(string coinId) => idToLoc.ContainsKey(coinId);
        public static long GetAPLocationId(string coinId) => idToLoc[coinId];
    }
}

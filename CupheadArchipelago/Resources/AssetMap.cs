/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class AssetMap {
        private static readonly Dictionary<string, HashSet<string>> assetMap = new() {
            {"cap_dicehouse", ["cap_dicehouse_chalkboard"]}
        };
        private static readonly Dictionary<string, string> assetToBundleMap = [];

        static AssetMap() {
            foreach (string resource in assetMap.Keys) {
                foreach (string asset in assetMap[resource]) {
                    assetToBundleMap.Add(asset, resource);
                }
            }
        }
    }
}

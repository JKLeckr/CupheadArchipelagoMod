/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class ResourceMap {
        private static readonly Dictionary<string, string> resourceMap = new() {
            {"atlas_cap_dicehouse", "cap_dicehouse"}
        };

        internal static IEnumerable<string> GetResourceNames() => resourceMap.Keys;
        internal static bool ResourceExists(string resourceName) => resourceMap.ContainsKey(resourceName);
        internal static string GetAssetBundleName(string resourceName) => resourceMap[resourceName];
    }
}

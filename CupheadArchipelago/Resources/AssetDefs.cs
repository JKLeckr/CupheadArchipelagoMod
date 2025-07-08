/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class AssetDefs {
        private static readonly Dictionary<string, HashSet<string>> assetBundleDefs = new() {
            { "testee", ["testee", "sqar", "a", "circ"] },
            { "cap_dicehouse", ["cap_dicehouse_chalkboard_tics"]},
        };
        private static readonly Dictionary<string, RAsset> assetDefs = new() {
            { "testee", new("testee", RAssetType.Sprite) },
            { "sqar", new("sqar", RAssetType.Sprite) },
            { "a", new("a", RAssetType.Sprite) },
            { "circ", new("circ", RAssetType.Sprite) },
            { "cap_dicehouse_chalkboard_tics", new("cap_dicehouse_chalkboard_tics", RAssetType.Sprite) },
        };
        private static readonly HashSet<string> persistentAssets = [];

        private static readonly Dictionary<string, string> assetToBundleMap = [];

        static AssetDefs() {
            foreach (string bundle in assetBundleDefs.Keys) {
                foreach (string asset in assetBundleDefs[bundle]) {
                    assetToBundleMap.Add(asset, bundle);
                }
            }
        }

        public static IEnumerable<string> GetAllRegisteredBundles() => assetBundleDefs.Keys;

        public static IEnumerable<string> GetAllRegisteredAssets() => assetDefs.Keys;

        public static IEnumerable<string> GetAssetsInBundle(string bundleName) => assetBundleDefs[bundleName];

        public static IEnumerable<string> GetPersisentAssets() => persistentAssets;

        public static string GetInternalAssetName(string assetName) => assetDefs[assetName].name;
        public static RAssetType GetAssetType(string assetName) => assetDefs[assetName].type;

        public static string GetBundleFromAsset(string assetName) =>
            assetToBundleMap.ContainsKey(assetName) ? assetToBundleMap[assetName] : null;

        public static bool IsAssetPersistent(string assetName) => persistentAssets.Contains(assetName);
    }
}

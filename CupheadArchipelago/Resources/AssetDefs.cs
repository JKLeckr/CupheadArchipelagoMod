/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class AssetDefs {
        // Register Assets and Bundles here
        // Bundles {{ bundleName, [ assetNames ] }}
        private static readonly Dictionary<string, HashSet<string>> assetBundleDefs = new() {
            { "testee", ["testee", "sqar", "a", "circ"] },
            { "cap_base", ["s_Debug", "s_TexOverlay"] },
            { "cap_dicehouse", ["cap_dicehouse_chalkboard_tics"]},
        };
        // Persistent Bundles [ bundleName ]
        private static readonly HashSet<string> persistentBundles = [
            "cap_base"
        ];
        // Assets {{ assetName, (internalAssetName, assetType) }}
        private static readonly Dictionary<string, RAsset> assetDefs = new() {
            { "testee", new("testee", RAssetType.Sprite) },
            { "sqar", new("sqar", RAssetType.Sprite) },
            { "a", new("a", RAssetType.Sprite) },
            { "circ", new("circ", RAssetType.Sprite) },
            { "s_Debug", new("Debug", RAssetType.Shader) },
            { "s_TexOverlay", new("TexOverlay", RAssetType.Shader) },
            { "cap_dicehouse_chalkboard_tics", new("cap_dicehouse_chalkboard_tics", RAssetType.Texture2D) },
        };
        // Persistent Assets [ assetName ]
        private static readonly HashSet<string> persistentAssets = [
            "s_Debug",
            "s_TexOverlay"
        ];

        // Auto generated (Do not change)
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

        public static IEnumerable<string> GetPersisentAssetBundles() => persistentBundles;
        public static IEnumerable<string> GetPersisentAssets() => persistentAssets;

        public static string GetInternalAssetName(string assetName) => assetDefs[assetName].name;
        public static RAssetType GetAssetType(string assetName) => assetDefs[assetName].type;

        public static string GetBundleFromAsset(string assetName) =>
            assetToBundleMap.ContainsKey(assetName) ? assetToBundleMap[assetName] : null;

        public static bool IsAssetBundlePersistent(string bundleName) => persistentBundles.Contains(bundleName);
        public static bool IsAssetPersistent(string assetName) => persistentAssets.Contains(assetName);
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class AssetReg {
        private static readonly Dictionary<string, HashSet<string>> assetReg = new() {
            {"cap_dicehouse", ["cap_dicehouse_chalkboard"]},
        };
        private static readonly Dictionary<string, RAssetType> assetTypes = new() {
            {"cap_dicehouse_chalkboard", RAssetType.Texture2D}
        };
        private static readonly HashSet<string> persistentAssets = [];

        private static readonly Dictionary<string, string> assetToBundleMap = [];

        static AssetReg() {
            foreach (string bundle in assetReg.Keys) {
                foreach (string asset in assetReg[bundle]) {
                    assetToBundleMap.Add(asset, bundle);
                }
            }
        }

        public static IEnumerable<string> GetAllRegisteredBundleNames() => assetReg.Keys;

        public static IEnumerable<string> GetAllRegisteredAssetNames() => assetToBundleMap.Keys;

        public static IEnumerable<string> GetAssetNamesInBundle(string bundleName) => assetReg[bundleName];

        public static IEnumerable<string> GetPersisentAssets() => persistentAssets;

        public static RAssetType GetAssetType(string assetName) =>
            assetTypes.ContainsKey(assetName) ? assetTypes[assetName] : RAssetType.Object;

        public static string GetBundleNamesFromAsset(string assetName) =>
            assetToBundleMap.ContainsKey(assetName) ? assetToBundleMap[assetName] : null;

        public static bool IsAssetPersistent(string assetName) => persistentAssets.Contains(assetName);
    }
}

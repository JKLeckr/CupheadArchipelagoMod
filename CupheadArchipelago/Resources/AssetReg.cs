/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class AssetReg {
        private static readonly Dictionary<string, HashSet<string>> assetReg = new() {
            {"cap_dicehouse", ["cap_dicehouse_chalkboard"]},
        };
        private static readonly Dictionary<string, RAssetType> assetTypes = new() {
            {"cap_dicehouse_chalkboard", RAssetType.Texture2D}
        };
        private static readonly HashSet<string> persistentAssets = [
            "cap_dicehouse_chalkboard",
        ];

        private static readonly Dictionary<string, string> assetToBundleMap = [];

        static AssetReg() {
            foreach (string bundle in assetReg.Keys) {
                foreach (string asset in assetReg[bundle]) {
                    /*if (assetToBundleMap.ContainsKey(asset)) {
                        throw new Exception(
                            $"Duplicate asset name in assetReg {asset}. Each asset name must be unique across bundles."
                        );
                    }*/
                    assetToBundleMap.Add(asset, bundle);
                }
            }
        }

        public static IEnumerable<string> GetAllRegisteredBundleNames() => assetReg.Keys;

        public static IEnumerable<string> GetAllRegisteredAssetNames() => assetToBundleMap.Keys;

        public static IEnumerable<string> GetAssetNamesInBundle(string bundleName) => assetReg[bundleName];

        public static RAssetType GetAssetType(string assetName) =>
            assetTypes.ContainsKey(assetName) ? assetTypes[assetName] : RAssetType.Object;

        public static string GetBundleNamesFromAsset(string assetName) => assetToBundleMap[assetName];

        public static bool IsAssetPersistent(string assetName) => persistentAssets.Contains(assetName);
    }
}

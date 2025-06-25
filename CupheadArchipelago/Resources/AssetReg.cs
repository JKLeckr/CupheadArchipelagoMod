/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class AssetReg {
        private static readonly Dictionary<string, HashSet<RAsset>> assetReg = new() {
            {
                "cap_dicehouse", [new("cap_dicehouse_chalkboard", RAssetType.Texture2D)]
            },
        };
        private static readonly HashSet<string> persistentAssets = [
            "cap_dicehouse_chalkboard",
        ];
        private static readonly Dictionary<string, string> assetToBundleMap = [];
        private static readonly HashSet<string> persistentBundles = [];

        static AssetReg() {
            foreach (string bundle in assetReg.Keys) {
                foreach (RAsset asset in assetReg[bundle]) {
                    assetToBundleMap.Add(asset.assetName, bundle);
                    if (persistentBundles.Contains(asset.assetName)) {
                        persistentBundles.Add(bundle);
                    }
                }
            }
        }

        private static void RefreshPersistentBundles() {
            foreach (KeyValuePair<string, string> asset in assetToBundleMap) {
                if (persistentAssets.Contains(asset.Key)) {
                    persistentBundles.Add(asset.Value);
                }
                else {
                    persistentBundles.Remove(asset.Value);
                }
            }
        }

        internal static string GetAssetBundle(string assetName) => assetToBundleMap[assetName];
        internal static bool IsAssetPersistent(string assetName) => persistentAssets.Contains(assetName);
        internal static bool IsAssetBundlePersistent(string assetBundleName) =>
            persistentBundles.Contains(assetBundleName);

        internal class RAsset(string assetName, RAssetType assetType) {
            internal readonly string assetName = assetName;
            internal readonly RAssetType assetType = assetType;
        }
    }
}

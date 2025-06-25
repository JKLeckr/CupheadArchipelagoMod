/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using CupheadArchipelago.Util;
using UnityEngine;

namespace CupheadArchipelago.Resources {
    internal class AssetMngr {
        private static readonly Dictionary<string, UnityEngine.Object> loadedAssets = [];

        internal static IEnumerator LoadSceneAssetsAsync(Scenes scene) =>
            LoadSceneAssetsAsync(scene.ToString());
        internal static IEnumerator LoadSceneAssetsAsync(string sceneName) {
            // TODO: Load Bundles when needed for asset
            yield break;
        }
        internal static IEnumerator LoadAssetAsync(string assetName) {
            yield break;
        }
        internal static IEnumerator LoadPersistentAssetsAsync() {
            yield break;
        }
        internal static void LoadSceneAssets(Scenes scene) =>
            LoadSceneAssets(scene.ToString());
        internal static void LoadSceneAssets(string sceneName) {
            //
        }
        internal static void LoadAsset(string assetName) {
            //
        }
        internal static void LoadPersistentAssets() { }
        internal static void UnloadAllAssets() => UnloadAllAssets(false);
        internal static void UnloadAllAssets(bool unloadPersistent) {
            foreach (KeyValuePair<string, UnityEngine.Object> loadedAsset in loadedAssets) {
                if (!AssetReg.IsAssetPersistent(loadedAsset.Key) || unloadPersistent) {
                    UnityEngine.Object.Destroy(loadedAsset.Value);
                    loadedAssets.Remove(loadedAsset.Key);
                }
            }
        }
        internal static void UnloadAssets(params string[] assets) {
            foreach (var asset in assets) {
                UnloadAsset(asset);
            }
        }
        private static void UnloadAsset(string assetName) {
            if (loadedAssets.ContainsKey(assetName)) {
                UnityEngine.Object.Destroy(loadedAssets[assetName]);
                loadedAssets.Remove(assetName);
            }
        }

        internal static bool IsAssetLoaded(string assetName) =>
            loadedAssets.ContainsKey(assetName);

        internal static T GetLoadedAsset<T>(string assetName) {
            if (IsAssetLoaded(assetName)) {
                UnityEngine.Object asset = loadedAssets[assetName];
                if (asset is T tAsset) {
                    return tAsset;
                }
                else {
                    throw new InvalidCastException($"Cannot load \"{assetName}\" as {typeof(T)}.");
                }
            }
            else {
                throw new KeyNotFoundException($"\"{assetName}\" is not loaded.");
            }
        }

        internal static string GetLoadedAssets() {
            LinkedList<string> resList = [];
            foreach (string asset in loadedAssets.Keys) {
                if (AssetReg.IsAssetPersistent(asset)) {
                    resList.AddFirst($"*{asset}");
                }
                else {
                    resList.AddLast(asset);
                }
            }
            return $"Loaded RAssets: {Aux.CollectionToString(resList)}";
        }
    }
}

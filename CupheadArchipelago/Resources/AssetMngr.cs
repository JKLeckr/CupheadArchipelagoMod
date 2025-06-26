/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using CupheadArchipelago.Util;
using UnityEngine;

namespace CupheadArchipelago.Resources {
    internal class AssetMngr {
        private static readonly Dictionary<RAssetType, Type> rAssetTypeMap = new() {
            {RAssetType.Object, typeof(UnityEngine.Object)},
            {RAssetType.GameObject, typeof(GameObject)},
            {RAssetType.Texture2D, typeof(Texture2D)},
        };

        private static readonly Dictionary<string, UnityEngine.Object> loadedAssets = [];

        internal static IEnumerator LoadSceneAssetsAsync(Scenes scene) =>
            LoadSceneAssetsAsync(scene.ToString());
        internal static IEnumerator LoadSceneAssetsAsync(string sceneName) {
            if (!AssetMap.IsSceneRegistered(sceneName)) {
                throw new KeyNotFoundException($"{sceneName} is not registered in Resource AssetMap.");
            }
            foreach (string assetName in AssetMap.GetSceneAssets(sceneName)) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    yield return LoadAssetAsync(assetName);
                }
                else {
                    Logging.Log($"{assetName} is already loaded.");
                }
            }
            yield break;
        }
        internal static IEnumerator LoadPersistentAssetsAsync() {
            foreach (string assetName in AssetReg.GetPersisentAssets()) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    yield return LoadAssetAsync(assetName);
                }
                yield return null;
            }
            yield break;
        }
        internal static IEnumerator LoadAllBundleAssetsAsync<T>(string bundleName) {
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                yield return AssetBundleMngr.LoadAssetBundleAsync(bundleName);
            }
            IEnumerable<string> bundleAssets = AssetReg.GetAssetNamesInBundle(bundleName);
            foreach (string asset in bundleAssets) {
                if (!loadedAssets.ContainsKey(asset)) {
                    yield return LoadAssetAsync(asset);
                }
                else {
                    Logging.Log($"{asset} is already loaded.");
                }
            }
            yield break;
        }
        internal static IEnumerator LoadAssetAsync(string assetName) {
            if (loadedAssets.ContainsKey(assetName)) {
                throw new Exception($"{assetName} is already loaded!");
            }
            string bundleName = AssetReg.GetBundleNamesFromAsset(assetName);
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                yield return AssetBundleMngr.LoadAssetBundleAsync(bundleName);
            }
            AssetBundle bundle = AssetBundleMngr.GetLoadedBundle(bundleName);
            AssetBundleRequest request =
                bundle.LoadAssetAsync(assetName, rAssetTypeMap[AssetReg.GetAssetType(assetName)]);
            yield return request;
            loadedAssets.Add(assetName, request.asset);
            yield break;
        }
        internal static void LoadSceneAssets(Scenes scene) =>
            LoadSceneAssets(scene.ToString());
        internal static void LoadSceneAssets(string sceneName) {
            if (!AssetMap.IsSceneRegistered(sceneName)) {
                throw new KeyNotFoundException($"{sceneName} is not registered in Resource AssetMap.");
            }
            foreach (string assetName in AssetMap.GetSceneAssets(sceneName)) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    LoadAsset(assetName);
                }
                else {
                    Logging.Log($"{assetName} is already loaded.");
                }
            }
        }
        internal static void LoadPersistentAssets() {
            foreach (string assetName in AssetReg.GetPersisentAssets()) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    LoadAsset(assetName);
                }
            }
        }
        internal static void LoadBundleAssets(string bundleName) {
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                AssetBundleMngr.LoadAssetBundle(bundleName);
            }
            IEnumerable<string> bundleAssets = AssetReg.GetAssetNamesInBundle(bundleName);
            foreach (string asset in bundleAssets) {
                if (!loadedAssets.ContainsKey(asset)) {
                    LoadAsset(asset);
                }
                else {
                    Logging.Log($"{asset} is already loaded.");
                }
            }
        }
        internal static void LoadAsset(string assetName) {
            if (loadedAssets.ContainsKey(assetName)) {
                throw new Exception($"{assetName} is already loaded!");
            }
            string bundleName = AssetReg.GetBundleNamesFromAsset(assetName);
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                AssetBundleMngr.LoadAssetBundle(bundleName);
            }
            AssetBundle bundle = AssetBundleMngr.GetLoadedBundle(bundleName);
            loadedAssets.Add(
                assetName,
                bundle.LoadAsset(assetName, rAssetTypeMap[AssetReg.GetAssetType(assetName)])
            );
        }
        internal static void UnloadAllAssets() => UnloadAllAssets(false);
        internal static void UnloadAllAssets(bool unloadPersistent) {
            foreach (string loadedAsset in loadedAssets.Keys) {
                if (!AssetReg.IsAssetPersistent(loadedAsset) || unloadPersistent) {
                    UnloadAsset(loadedAsset);
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

        internal static string GetLoadedAssetsAsString() {
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

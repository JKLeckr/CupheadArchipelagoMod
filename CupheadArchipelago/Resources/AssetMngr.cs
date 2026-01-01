/// Copyright 2025-2026 JKLeckr
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
            {RAssetType.Sprite, typeof(Texture2D)},
            {RAssetType.Shader, typeof(Shader)},
        };

        private static readonly Dictionary<string, UnityEngine.Object> loadedAssets = [];

        internal static IEnumerator LoadSceneAssetsAsync(Scenes scene) =>
            LoadSceneAssetsAsync(scene.ToString());
        internal static IEnumerator LoadSceneAssetsAsync(string sceneName) {
            if (!SceneAssetMap.IsSceneRegistered(sceneName)) {
                throw new KeyNotFoundException($"{sceneName} is not registered in Resource AssetMap.");
            }
            foreach (string assetName in SceneAssetMap.GetSceneAssets(sceneName)) {
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
            foreach (string assetName in AssetDefs.GetPersisentAssets()) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    yield return LoadAssetAsync(assetName);
                }
                yield return null;
            }
            yield break;
        }
        internal static IEnumerator LoadBundleAssetsAsync<T>(string bundleName) {
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                yield return AssetBundleMngr.LoadAssetBundleAsync(bundleName);
            }
            IEnumerable<string> bundleAssets = AssetDefs.GetAssetsInBundle(bundleName);
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
            string bundleName = AssetDefs.GetBundleFromAsset(assetName) ??
                throw new NullReferenceException($"{assetName} does not exist in an asset bundle!");
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                yield return AssetBundleMngr.LoadAssetBundleAsync(bundleName);
            }
            AssetBundle bundle = AssetBundleMngr.GetLoadedBundle(bundleName);
            RAssetType assetType = AssetDefs.GetAssetType(assetName);
            AssetBundleRequest request =
                bundle.LoadAssetAsync(
                    AssetDefs.GetInternalAssetName(assetName),
                    rAssetTypeMap[assetType]
                );
            yield return request;
            if (request.asset == null) {
                throw new Exception($"Asset \"{assetName}\" from Bundle \"{bundleName}\" could not be loaded!");
            }
            ProcessAsset(request.asset, assetType, out UnityEngine.Object asset);
            loadedAssets.Add(assetName, asset);
            yield break;
        }
        internal static void LoadSceneAssets(Scenes scene) =>
            LoadSceneAssets(scene.ToString());
        internal static void LoadSceneAssets(string sceneName) {
            if (!SceneAssetMap.IsSceneRegistered(sceneName)) {
                throw new KeyNotFoundException($"{sceneName} is not registered in Resource AssetMap.");
            }
            foreach (string assetName in SceneAssetMap.GetSceneAssets(sceneName)) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    LoadAsset(assetName);
                }
                else {
                    Logging.Log($"{assetName} is already loaded.");
                }
            }
        }
        internal static void LoadPersistentAssets() {
            foreach (string assetName in AssetDefs.GetPersisentAssets()) {
                if (!loadedAssets.ContainsKey(assetName)) {
                    LoadAsset(assetName);
                }
            }
        }
        internal static void LoadBundleAssets(string bundleName) {
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                AssetBundleMngr.LoadAssetBundle(bundleName);
            }
            IEnumerable<string> bundleAssets = AssetDefs.GetAssetsInBundle(bundleName);
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
            string bundleName = AssetDefs.GetBundleFromAsset(assetName) ??
                throw new NullReferenceException($"{assetName} does not exist in an asset bundle!");
            if (!AssetBundleMngr.IsAssetBundleLoaded(bundleName)) {
                AssetBundleMngr.LoadAssetBundle(bundleName);
            }
            AssetBundle bundle = AssetBundleMngr.GetLoadedBundle(bundleName);
            UnityEngine.Object rasset = bundle.LoadAsset(
                AssetDefs.GetInternalAssetName(assetName),
                rAssetTypeMap[AssetDefs.GetAssetType(assetName)]
            ) ?? throw new Exception($"Asset \"{assetName}\" from Bundle \"{bundleName}\" could not be loaded!");
            ProcessAsset(
                rasset,
                AssetDefs.GetAssetType(assetName),
                out UnityEngine.Object asset
            );
            loadedAssets.Add(assetName, asset);
        }
        internal static void UnloadAllAssets() => UnloadAllAssets(false);
        internal static void UnloadAllAssets(bool unloadPersistent) {
            string[] loadedAssetNames = [.. loadedAssets.Keys];
            foreach (string loadedAsset in loadedAssetNames) {
                if (!AssetDefs.IsAssetPersistent(loadedAsset) || unloadPersistent) {
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
                if (AssetDefs.IsAssetPersistent(asset)) {
                    resList.AddFirst($"*{asset}");
                }
                else {
                    resList.AddLast(asset);
                }
            }
            return $"Loaded RAssets: {Aux.CollectionToString(resList)}";
        }

        private static bool ProcessAsset(UnityEngine.Object asset, RAssetType assetType, out UnityEngine.Object nAsset) {
            switch (assetType) {
                case RAssetType.Sprite:
                    Texture2D tex = (Texture2D)asset;
                    nAsset = Sprite.Create(tex, new(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    return true;
                default:
                    nAsset = asset;
                    return false;
            }
        }
    }
}

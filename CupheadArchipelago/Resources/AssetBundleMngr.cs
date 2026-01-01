/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using CupheadArchipelago.Util;
using UnityEngine;

namespace CupheadArchipelago.Resources {
    internal class AssetBundleMngr {
        private static readonly Dictionary<string, AssetBundle> loadedBundles = [];

        internal static IEnumerator LoadPersistentAssetBundlesAsync() {
            foreach (string bundleName in AssetDefs.GetPersisentAssetBundles()) {
                if (!loadedBundles.ContainsKey(bundleName)) {
                    yield return LoadAssetBundleAsync(bundleName);
                }
                yield return null;
            }
            yield break;
        }
        internal static IEnumerator LoadAssetBundleAsync(string bundleName) {
            if (loadedBundles.ContainsKey(bundleName)) {
                Logging.LogError($"{bundleName} is already loaded!");
            }
            AssetBundleCreateRequest request =
                AssetBundle.LoadFromMemoryAsync(ResourceLoader.GetLoadedResource(bundleName));
            yield return request;
            loadedBundles.Add(bundleName, request.assetBundle);
            yield break;
        }
        internal static void LoadPersistentAssetBundles() {
            foreach (string bundleName in AssetDefs.GetPersisentAssetBundles()) {
                if (!loadedBundles.ContainsKey(bundleName)) {
                    LoadAssetBundle(bundleName);
                }
            }
        }
        internal static void LoadAssetBundle(string bundleName) {
            if (loadedBundles.ContainsKey(bundleName)) {
                Logging.LogError($"{bundleName} is already loaded!");
            }
            loadedBundles.Add(
                bundleName, AssetBundle.LoadFromMemory(ResourceLoader.GetLoadedResource(bundleName))
            );
        }
        internal static void UnloadAssetBundles() => UnloadAssetBundles(false);
        internal static void UnloadAssetBundles(bool unloadPersistent) {
            string[] loadedBundleNames = [.. loadedBundles.Keys];
            foreach (string bundleName in loadedBundleNames) {
                if (!AssetDefs.IsAssetBundlePersistent(bundleName) || unloadPersistent)
                    UnloadAssetBundle(bundleName);
            }
        }
        internal static void UnloadAssetBundle(string bundleName) {
            loadedBundles[bundleName].Unload(false);
            loadedBundles.Remove(bundleName);
        }

        internal static bool IsAssetBundleLoaded(string bundleName) =>
            loadedBundles.ContainsKey(bundleName);

        internal static AssetBundle GetLoadedBundle(string bundleName) {
            if (!loadedBundles.ContainsKey(bundleName)) {
                throw new KeyNotFoundException($"{bundleName} is not loaded!");
            }
            return loadedBundles[bundleName];
        }

        internal static string GetLoadedAssetBundlesAsString() {
            return $"Loaded RBundles: {Aux.CollectionToString(loadedBundles)}";
        }
    }
}

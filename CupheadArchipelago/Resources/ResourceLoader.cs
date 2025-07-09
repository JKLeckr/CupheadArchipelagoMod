/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CupheadArchipelago.Resources {
    internal class ResourceLoader {
        private static readonly Dictionary<string, byte[]> loadedResources = [];

        internal const string RESOURCE_PRE = "CupheadArchipelago.Assets.";

        public static void LoadResources() {
            foreach (string resource in ResourceDefs.GetRegisteredResources()) {
                Logging.LogDebug($"Loading resource {resource}...");
                byte[] resoureBytes = GetResourceBytes(RESOURCE_PRE + resource);
                loadedResources.Add(resource, resoureBytes);
                Logging.LogDebug($"Loaded resource {resource}.");
            }
            Logging.LogDebug($"Loading persistent resource assets...");
            AssetBundleMngr.LoadPersistentAssetBundles();
            AssetMngr.LoadPersistentAssets();
            StaticAssets.Init();
        }

        private static byte[] GetResourceBytes(string resourceName) {
            Assembly asm = typeof(ResourceLoader).Assembly;
            byte[] buffer = new byte[16384];
            using Stream stream = asm.GetManifestResourceStream(resourceName);
            using MemoryStream ms = new();
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        internal static byte[] GetLoadedResource(string name) {
            if (loadedResources.ContainsKey(name))
                return loadedResources[name];
            else
                throw new KeyNotFoundException($"Resource \"{name}\" not loaded.");
        }
    }
}

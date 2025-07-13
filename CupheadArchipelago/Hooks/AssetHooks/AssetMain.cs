/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.AssetHooks {
    public class AssetMain {
        public static void Hook() {
            AssetLoaderHook.Hook();
            AssetBundleLoaderHook.Hook();
        }
    }
}

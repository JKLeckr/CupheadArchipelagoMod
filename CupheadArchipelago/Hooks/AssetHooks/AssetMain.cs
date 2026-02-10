/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.AssetHooks {
    public class AssetMain {
        public static void Hook() {
            AssetLoaderHook.Hook();
            AssetBundleLoaderHook.Hook();
        }
    }
}

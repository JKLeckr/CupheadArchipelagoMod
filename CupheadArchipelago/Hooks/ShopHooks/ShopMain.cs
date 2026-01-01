/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.ShopHooks {
    public class ShopMain {
        public static void Hook() {
            ShopSceneHook.Hook();
            ShopSceneItemHook.Hook();
            ShopScenePlayerHook.Hook();
        }
    }
}

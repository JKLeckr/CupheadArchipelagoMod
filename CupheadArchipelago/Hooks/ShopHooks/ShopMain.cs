/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.ShopHooks {
    public class ShopMain {
        public static void Hook() {
            ShopSceneHook.Hook();
            ShopSceneItemHook.Hook();
            ShopScenePlayerHook.Hook();
        }
    }
}

/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.PlayerHooks.PlanePlayerHooks {
    public class PlanePlayerMain {
        public static void Hook() {
            PlanePlayerControllerHook.Hook();
            PlanePlayerAnimationControllerHook.Hook();
            PlanePlayerParryControllerHook.Hook();
            PlanePlayerWeaponManagerHook.Hook();
            PlaneWeaponHook.Hook();
        }
    }
}

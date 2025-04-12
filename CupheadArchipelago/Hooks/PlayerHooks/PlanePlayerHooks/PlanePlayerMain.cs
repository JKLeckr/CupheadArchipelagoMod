/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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

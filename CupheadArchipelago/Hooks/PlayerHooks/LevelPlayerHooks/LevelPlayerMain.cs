/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    public class LevelPlayerMain {
        public static void Hook() {
            LevelPlayerControllerHook.Hook();
            LevelPlayerMotorHook.Hook();
            LevelPlayerAnimationControllerHook.Hook();
            LevelPlayerWeaponManagerHook.Hook();
            LevelWeaponHook.Hook();
        }
    }
}

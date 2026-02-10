/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

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

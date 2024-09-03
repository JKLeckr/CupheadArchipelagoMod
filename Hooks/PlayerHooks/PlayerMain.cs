/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.PlayerHooks {
    public class PlayerMain {
        public static void Hook() {
            PlayerStatsManagerHook.Hook();
            PlanePlayerControllerHook.Hook();
            LevelPlayerMotorHook.Hook();
            LevelPlayerAnimationControllerHook.Hook();
            PlanePlayerAnimationControllerHook.Hook();
            PlanePlayerParryControllerHook.Hook();
            LevelPlayerWeaponManagerHook.Hook();
            PlanePlayerWeaponManagerHook.Hook();
        }
    }
}
/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.PlayerHooks {
    public class PlayerMain {
        public static void Hook() {
            PlayerStatsManagerHook.Hook();
            PlanePlayerControllerHook.Hook();
            LevelPlayerWeaponManagerHook.Hook();
            PlanePlayerWeaponManagerHook.Hook();
        }
    }
}
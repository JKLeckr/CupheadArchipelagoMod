/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.PlayerHooks {
    public class PlayerMain {
        public static void Hook() {
            PlayerStatsManagerHook.Hook();
            PlayerDeathEffectHook.Hook();
            LevelPlayerHooks.LevelPlayerMain.Hook();
            PlanePlayerHooks.PlanePlayerMain.Hook();
        }
    }
}

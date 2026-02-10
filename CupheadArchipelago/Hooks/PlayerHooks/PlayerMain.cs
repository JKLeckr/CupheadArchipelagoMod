/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.PlayerHooks {
    public class PlayerMain {
        public static void Hook() {
            PlayerStatsManagerHook.Hook();
            LevelPlayerHooks.LevelPlayerMain.Hook();
            PlanePlayerHooks.PlanePlayerMain.Hook();
        }
    }
}

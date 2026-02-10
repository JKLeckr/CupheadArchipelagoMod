/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.LevelHooks {
    public class LevelMain {
        public static void Hook() {
            LevelHook.Hook();
            HouseLevelHook.Hook();
            TutorialLevelDoorHook.Hook();
            PlatformingLevelHook.Hook();
            LevelCoinHook.Hook();
            MausoleumLevelHook.Hook();
            DiceGateLevelHook.Hook();
            DiceGateLevelToNextWorldHook.Hook();
            DicePalaceMainLevelGameManagerHook.Hook();
            KitchenLevelHook.Hook();
            KitchenSaltbakerCounterHook.Hook();
            ChessCastleLevelHook.Hook();
            AirplaneLevelHook.Hook();
        }
    }
}

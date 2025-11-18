/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks {
    public class Main {
        public static void HookMain() {
            CupheadHook.Hook();
            StartScreenHook.Hook();
            SlotSelectScreenHook.Hook();
            SlotSelectScreenSlotHook.Hook();
            PlayerDataHook.Hook();
            PlayerStatsManagerHook.Hook();
            MapHook.Hook();
            MapLevelDependentObstacleHook.Hook();
            MapCoinHook.Hook();
            MapNPCAppletravellerHook.Hook();
            MapDifficultySelectStartUIHook.Hook();
            MapUICoinHook.Hook();
            LevelHook.Hook();
            TutorialLevelDoorHook.Hook();
            PlatformingLevelHook.Hook();
            LevelCoinHook.Hook();
            MausoleumLevelHook.Hook();
            DiceGateLevelHook.Hook();
        }

        public static void HookSaveKeyUpdater(string saveKeyName) {
            SaveKeyUpdaterHook.SetSaveKeyBaseName(saveKeyName);
            SaveKeyUpdaterHook.Hook();
        }
    }
}
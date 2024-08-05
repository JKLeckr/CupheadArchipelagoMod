/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.LevelHooks {
    public class LevelMain {
        public static void Hook() {
            LevelHook.Hook();
            TutorialLevelDoorHook.Hook();
            PlatformingLevelHook.Hook();
            LevelCoinHook.Hook();
            MausoleumLevelHook.Hook();
            DiceGateLevelHook.Hook();
        }
    }
}

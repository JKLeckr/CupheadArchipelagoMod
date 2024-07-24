/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.MapHooks {
    public class MapMain {
        public static void Hook() {
            MapHook.Hook();
            MapLevelDependentObstacleHook.Hook();
            MapCoinHook.Hook();
            MapNPCAppletravellerHook.Hook();
            MapNPCCanteenHook.Hook();
            MapDifficultySelectStartUIHook.Hook();
            MapUICoinHook.Hook();
        }
    }
}
/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Hooks.MapHooks.MapNPCHooks;

namespace CupheadArchipelago.Hooks.MapHooks {
    public class MapMain {
        public static void Hook() {
            MapHook.Hook();
            MapLevelDependentObstacleHook.Hook();
            MapCoinHook.Hook();
            MapLevelLoaderHook.Hook();
            MapDifficultySelectStartUIHook.Hook();
            MapLevelMausoleumEntityHook.Hook();
            MapUICoinsHook.Hook();
            MapShmupTutorialBridgeActivatorHook.Hook();
            MapNPCMain.Hook();
        }
    }
}
/// Copyright 2025 JKLeckr
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
            MapEquipUIChecklistHook.Hook();
            MapEquipUIChecklistItemHook.Hook();
            MapLevelMausoleumEntityHook.Hook();
            MapUICoinsHook.Hook();
            MapShmupTutorialBridgeActivatorHook.Hook();
            BoatmanEnablerHook.Hook();
            MapNPCMain.Hook();
        }
    }
}
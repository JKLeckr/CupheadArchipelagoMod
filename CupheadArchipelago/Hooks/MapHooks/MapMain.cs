/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.MapHooks {
    public class MapMain {
        public static void Hook() {
            AbstractMapLevelDependentEntityHook.Hook();
            MapHook.Hook();
            MapEventNotificationHook.Hook();
            MapLevelDependentObstacleHook.Hook();
            MapCoinHook.Hook();
            MapLevelLoaderHook.Hook();
            MapDifficultySelectStartUIHook.Hook();
            MapEquipUIChecklistHook.Hook();
            MapLevelMausoleumEntityHook.Hook();
            MapUICoinsHook.Hook();
            MapShmupTutorialBridgeActivatorHook.Hook();
            BoatmanEnablerHook.Hook();
            MapDLCHook.Hook();
            MapNPCHooks.MapNPCMain.Hook();
        }
    }
}

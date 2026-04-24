/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.MapHooks.MapUIHooks {
    public class MapUIMain {
        public static void Hook() {
            MapUICoinsHook.Hook();
            MapEventNotificationHook.Hook();
            MapDifficultySelectStartUIHook.Hook();
            MapEquipUICardBackSelectHook.Hook();
            MapEquipUICardBackSelectIconHook.Hook();
            MapEquipUIChecklistHook.Hook();
        }
    }
}

/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    public class MapNPCMain {
        public static void Hook() {
            MapNPCAppletravellerHook.Hook();
            MapNPCAxemanHook.Hook();
            MapNPCBarbershopHook.Hook();
            MapNPCBarbershopSongHook.Hook();
            MapNPCCanteenHook.Hook();
            MapNPCCircusgirlHook.Hook();
            MapNPCJugglerHook.Hook();
            MapNPCMusicHook.Hook();
            MapNPCProfessionalHook.Hook();
            MapNPCTurtleHook.Hook();
            MapNPCBoatmanHook.Hook();
            MapNPCNewsieCatHook.Hook();
            MapNPCChaliceFanBHook.Hook();
        }
    }
}

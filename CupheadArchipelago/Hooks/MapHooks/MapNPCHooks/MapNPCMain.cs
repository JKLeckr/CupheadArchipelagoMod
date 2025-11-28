/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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

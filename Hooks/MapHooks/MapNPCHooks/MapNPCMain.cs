/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    public class MapNPCMain {
        public static void Hook() {
            MapNPCAppletravellerHook.Hook();
            MapNPCBarbershopHook.Hook();
            MapNPCBarbershopSongHook.Hook();
            MapNPCCanteenHook.Hook();
            MapNPCCircusgirlHook.Hook();
            MapNPCJugglerHook.Hook();
            MapNPCMusicHook.Hook();
            MapNPCProfessionalHook.Hook();
            MapNPCTurtleHook.Hook();
        }
    }
}
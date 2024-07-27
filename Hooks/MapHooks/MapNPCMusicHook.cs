/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapNPCMusicHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(MapNPCMusicHook));
        }

        [HarmonyPatch(typeof(MapNPCMusic), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            private static void APCheck() {
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.Check(APLocation.quest_music);
                }
            }
        }
    }
}

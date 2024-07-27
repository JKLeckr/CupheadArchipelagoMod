/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCBarbershopSongHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_4mel;

        [HarmonyPatch(typeof(MapNPCBarbershopSong), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "QuartetSing") {
                        if (!APClient.IsLocationChecked(locationId)) {
                            APClient.Check(locationId);
                            PlayerData.SaveCurrentFile();
                        }
                    }
                }
                return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Plugin.Log($"{nameof(MapNPCBarbershopSong)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

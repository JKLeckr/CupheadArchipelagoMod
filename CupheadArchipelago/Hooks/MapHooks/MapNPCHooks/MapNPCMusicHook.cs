/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCMusicHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        [HarmonyPatch(typeof(MapNPCMusic), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static void Postfix(string message, ref MapNPCMusic.MusicType ___musicType) {
                if (message == "MinimalistMusic" && ___musicType == MapNPCMusic.MusicType.Minimalist) {
                    Logging.Log("Music M");
	            }
	            else if (message == "RegularMusic" && ___musicType == MapNPCMusic.MusicType.Regular) {
                    Logging.Log("Music R");
                }
            }
            private static void APCheck() {
                if (APData.IsCurrentSlotEnabled() && !APClient.IsLocationChecked(APLocation.quest_music)) {
                    APClient.Check(APLocation.quest_music);
                }
            }
        }
    }
}

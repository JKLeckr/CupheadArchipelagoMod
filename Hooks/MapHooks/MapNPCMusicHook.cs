/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapNPCMusicHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        [HarmonyPatch(typeof(MapNPCMusic), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static void Postfix(string message, ref MapNPCMusic.MusicType __musicType) {
                if (message == "MinimalistMusic" && __musicType == MapNPCMusic.MusicType.Minimalist) {
                    Plugin.Log("Music M");
	            }
	            else if (message == "RegularMusic" && __musicType == MapNPCMusic.MusicType.Regular) {
                    Plugin.Log("Music R");
                }
            }
            private static void APCheck() {
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.Check(APLocation.quest_music);
                }
            }
        }
    }
}

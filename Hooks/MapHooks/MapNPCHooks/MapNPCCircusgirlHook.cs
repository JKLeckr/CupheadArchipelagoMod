/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Drawing.Printing;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCCircusgirlHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_ginger;

        [HarmonyPatch(typeof(MapNPCCircusgirl), "Start")]
        internal static class Start {
            static bool Prefix(MapNPCCircusgirl __instance, int ___dialoguerVariableID) {
                if (APData.IsCurrentSlotEnabled()) {
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                    if (APClient.IsLocationChecked(locationId)) {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 2f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                    }
                    else {
                        __instance.AddDialoguerEvents();
                    }
                    return false;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCCircusgirl), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                Logging.Log($"CircusGirl: \"{message}\" {___SkipDialogueEvent}");
                if (APData.IsCurrentSlotEnabled()) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "GingerbreadCoin") {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 2f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        //MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Coin);
                    }
                    return false;
                }
                else return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCCircusgirl)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

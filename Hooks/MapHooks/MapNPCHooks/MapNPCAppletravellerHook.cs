/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCAppletravellerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.npc_mac;

        [HarmonyPatch(typeof(MapNPCAppletraveller), "Start")]
        internal static class Start {
            static bool Prefix(int ___dialoguerVariableID) {
                if (APData.IsCurrentSlotEnabled()) {
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                    if (APClient.IsLocationChecked(locationId)) {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 1f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                    }
                } 
                return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCAppletraveller), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "MacCoin") {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 1f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        //MapEventNotification.Current.ShowEvent(MapEventNotification.Type.ThreeCoins);
                    }
                    return false;
                }
                else return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCAppletraveller)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

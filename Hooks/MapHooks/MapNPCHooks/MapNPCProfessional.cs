/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCProfessionalHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_15agrades;

        [HarmonyPatch(typeof(MapNPCProfessional), "Start")]
        internal static class Start {
            static bool Prefix(int ___dialoguerVariableID) {
                LogDialoguerGlobalFloat(___dialoguerVariableID);
                if (APData.IsCurrentSlotEnabled()) {
                    if (APClient.IsLocationChecked(locationId)) {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 3f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCProfessional), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "RetroColorUnlock") {
                        MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Professional);
		                PlayerData.Data.unlocked2Strip = true;
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        MapUI.Current.Refresh();
                    }
                }
                return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Plugin.Log($"{nameof(MapNPCProfessional)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

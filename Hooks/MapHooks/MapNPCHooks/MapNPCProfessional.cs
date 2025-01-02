/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCProfessionalHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_silverworth;

        [HarmonyPatch(typeof(MapNPCProfessional), "Start")]
        internal static class Start {
            static bool Prefix(int ___dialoguerVariableID) {
                LogDialoguerGlobalFloat(___dialoguerVariableID);
                if (APData.IsCurrentSlotEnabled() && APSettings.QuestProfessional) {
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
                if (APData.IsCurrentSlotEnabled() && APSettings.QuestProfessional) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "RetroColorUnlock") {
                        MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Professional);
		                PlayerData.Data.unlocked2Strip = true;
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        MapUI.Current.Refresh();
                    }
                    return false;
                }
                return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCProfessional)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

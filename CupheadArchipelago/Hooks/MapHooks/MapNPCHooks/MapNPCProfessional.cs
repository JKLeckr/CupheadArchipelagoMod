/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection.Emit;
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
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                return MapNPCHookBase.
                        MapNPCQuestHookBase.
                        MapNPCQuestHookTranspiler(instructions, locationId);
            }
            static void Postfix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                if (___SkipDialogueEvent) return;
                if (message == "RetroColorUnlock") {
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                }
            }
        }
        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCProfessional)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

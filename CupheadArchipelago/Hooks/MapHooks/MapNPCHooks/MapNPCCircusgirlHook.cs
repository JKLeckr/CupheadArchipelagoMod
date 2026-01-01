/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection.Emit;
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
                    if (APClient.IsLocationChecked(locationId)) {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 2f);
                    }
                    else {
                        __instance.AddDialoguerEvents();
                    }
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                    return false;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCCircusgirl), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                return MapNPCHookBase.
                        MapNPCCoinHookBase.
                        MapNPCCoinHookTranspiler(instructions, il, locationId);
            }
            static void Postfix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                if (___SkipDialogueEvent) return;
                if (message == "GingerbreadCoin") {
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                }
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCCircusgirl)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

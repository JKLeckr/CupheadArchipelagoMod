/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class ChessCastleLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(onDialogueEndedHandler));
            Harmony.CreateAndPatchAll(typeof(onDialogueMessageHandler));
        }

        private static readonly long locationIdRun = APLocation.level_dlc_chesscastle_run;

        [HarmonyPatch(typeof(ChessCastleLevel), "onDialogueEndedHandler")]
        internal static class onDialogueEndedHandler {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                MethodInfo _mi_APCheck = typeof(onDialogueEndedHandler).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                int ins_index = codes.Count-1;

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                List<Label> end_labels = codes[ins_index].labels;
                codes[ins_index].labels = [];
                List<CodeInstruction> ncodes = [
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _mi_APCheck),
                ];
                codes.InsertRange(ins_index, ncodes);
                codes[ins_index].labels = end_labels;
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static void APCheck(bool cond) {
                if (APData.IsCurrentSlotEnabled() && cond) {
                    if (!APClient.IsLocationChecked(locationIdRun))
                        APClient.Check(locationIdRun);
                }
            }
        }

        [HarmonyPatch(typeof(ChessCastleLevel), "onDialogueMessageHandler")]
        internal static class onDialogueMessageHandler {
            static bool Prefix(ref string message) {
                if (message == "GiveCoins") message = "_GiveCoins";
                return true;
            }
        }
    }
}

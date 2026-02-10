/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class ChessCastleLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(onDialogueEndedHandler));
            //Harmony.CreateAndPatchAll(typeof(onDialogueMessageHandler));
            //Harmony.CreateAndPatchAll(typeof(postWinEntry_cr));
        }

        private static readonly long locationIdRun = APLocation.level_dlc_chesscastle_run;

        [HarmonyPatch(typeof(ChessCastleLevel), "Awake")]
        internal static class Awake {
            static void Postfix(ChessCastleLevel __instance) {
                if (APData.IsCurrentSlotEnabled() && Level.Won) {
                    __instance.StartCoroutine(SendChecks_cr());
                }
            }

            private static IEnumerator SendChecks_cr() {
                while (SceneLoader.CurrentlyLoading) {
                    yield return null;
                }
                APClient.SendChecks(true);
            }
        }

        [HarmonyPatch(typeof(ChessCastleLevel), "onDialogueEndedHandler")]
        internal static class onDialogueEndedHandler {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                MethodInfo _mi_APCheck = typeof(onDialogueEndedHandler).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                int ins_index = codes.Count-1;

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                List<Label> end_labels = codes[ins_index].labels;
                codes[ins_index].labels = [];
                CodeInstruction[] ncodes = [
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _mi_APCheck),
                ];
                codes.InsertRange(ins_index, ncodes);
                codes[ins_index].labels = end_labels;
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
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
            static bool Prefix(string message) {
                return !(APData.IsCurrentSlotEnabled() && message == "GiveCoins");
            }
        }

        [HarmonyPatch(typeof(ChessCastleLevel), "postWinEntry_cr", MethodType.Enumerator)]
        internal static class postWinEntry_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_WaitForSeconds = typeof(CupheadTime).GetMethod(
                    "WaitForSeconds",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(MonoBehaviour), typeof(float)],
                    null
                );
                MethodInfo _mi_APWaitTime = typeof(postWinEntry_cr).GetMethod("APWaitTime", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldarg_0 && codes[i+2].opcode == OpCodes.Ldfld && codes[i+3].opcode == OpCodes.Ldc_R4 &&
                        (float)codes[i+3].operand == 2f && codes[i+4].opcode == OpCodes.Call && (MethodInfo)codes[i+4].operand == _mi_WaitForSeconds) {
                            codes[i+3] = new CodeInstruction(OpCodes.Call, _mi_APWaitTime);
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(postWinEntry_cr)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static float APWaitTime() {
                return APData.IsCurrentSlotEnabled() ? 0.2f : 2f;
            }
        }
    }
}

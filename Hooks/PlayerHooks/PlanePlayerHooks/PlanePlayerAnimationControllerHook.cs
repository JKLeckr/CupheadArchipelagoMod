/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.PlanePlayerHooks {
    internal class PlanePlayerAnimationControllerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(HandleShrunk));
        }

        [HarmonyPatch(typeof(PlanePlayerAnimationController), "HandleShrunk")]
        internal static class HandleShrunk {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_ShrinkState = typeof(PlanePlayerAnimationController).GetProperty("ShrinkState", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_APIsShrinkReady = typeof(HandleShrunk).GetMethod("APIsShrinkReady", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0; i<codes.Count-2;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_get_ShrinkState && codes[i+2].opcode == OpCodes.Brtrue) {
                        codes.Insert(i+2, new CodeInstruction(OpCodes.Call, _mi_APIsShrinkReady));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleShrunk)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APIsShrinkReady(PlanePlayerAnimationController.ShrinkStates state) {
                return state != 0 || (APData.IsCurrentSlotEnabled() && !APData.CurrentSData.playerData.plane_shrink);
            }
        }
    }
}

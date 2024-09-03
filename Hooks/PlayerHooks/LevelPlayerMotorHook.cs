/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class LevelPlayerMotorHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Ducking));
            Harmony.CreateAndPatchAll(typeof(HandleDash));
            //Harmony.CreateAndPatchAll(typeof(HandleLooking));
            Harmony.CreateAndPatchAll(typeof(HandleParry));
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "Ducking", MethodType.Getter)]
        internal static class Ducking {
            static bool Prefix(ref bool __result) {
                if (APData.IsCurrentSlotEnabled() && !APData.CurrentSData.playerData.duck) {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleDash")]
        internal static class HandleDash {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_dashManager = typeof(LevelPlayerMotor).GetField("dashManager", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_DM_state = typeof(LevelPlayerMotor.DashManager).GetField("state", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APIsDashUnReady = typeof(HandleDash).GetMethod("APIsDashUnReady", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0; i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_dashManager &&
                        codes[i+2].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+2].operand == _fi_DM_state && codes[i+3].opcode == OpCodes.Brtrue) {
                            codes.Insert(i+3, new CodeInstruction(OpCodes.Call, _mi_APIsDashUnReady));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleDash)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APIsDashUnReady(LevelPlayerMotor.DashManager.State state) {
                return state != 0 || (APData.IsCurrentSlotEnabled() && !APData.CurrentSData.playerData.dash);
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleLooking")]
        internal static class HandleLooking {
            static bool Prefix() {
                return true;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleParry")]
        internal static class HandleParry {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APData.CurrentSData.playerData.parry;
            }
        }
    }
}

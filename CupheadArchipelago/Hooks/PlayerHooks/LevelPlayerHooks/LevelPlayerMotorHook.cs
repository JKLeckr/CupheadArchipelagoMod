/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerMotorHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(HandleDash));
            Harmony.CreateAndPatchAll(typeof(HandleLooking));
            Harmony.CreateAndPatchAll(typeof(HandleJumping));
            Harmony.CreateAndPatchAll(typeof(HandleParry));
            Harmony.CreateAndPatchAll(typeof(ChaliceDashParry));
            Harmony.CreateAndPatchAll(typeof(ChaliceDoubleJump));
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
                    Dbg.LogCodeInstructions(codes);
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
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APIsDashUnReady(LevelPlayerMotor.DashManager.State state) {
                return state != 0 || (APData.IsCurrentSlotEnabled() && !APData.CurrentSData.playerData.dash);
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleJumping")]
        internal static class HandleJumping {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_canFallThrough = typeof(LevelPlatform).GetField("canFallThrough", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_GetComponent =
                    typeof(Component).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(
                        m => m.Name.Equals("GetComponent") && m.IsGenericMethod
                    )?.MakeGenericMethod(typeof(LevelPlatform));

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 7; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Callvirt &&
                        codes[i + 3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 3].operand == _mi_GetComponent &&
                        codes[i + 4].opcode == OpCodes.Stloc_2 && codes[i + 5].opcode == OpCodes.Ldloc_2 && codes[i + 6].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i + 6].operand == _fi_canFallThrough && codes[i + 7].opcode == OpCodes.Brfalse
                    ) {
                        Label l_skipdrop = (Label)codes[i + 7].operand;
                        List<CodeInstruction> ncodes = [
                            CodeInstruction.Call(() => APPlatformDropCondition()),
                            new(OpCodes.Brfalse, l_skipdrop),
                        ];
                        codes.InsertRange(i, ncodes);
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleJumping)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APPlatformDropCondition() {
                if (APData.IsCurrentSlotEnabled()) {
                    return APClient.APSessionGSPlayerData.duck || APSettings.DuckLockPlatDropBug;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleLooking")]
        internal static class HandleLooking {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_set_LookDirection = typeof(LevelPlayerMotor).GetProperty("LookDirection", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);
                MethodInfo _mi_DuckHack = typeof(HandleLooking).GetMethod("DuckHack", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldloc_0 && codes[i+2].opcode == OpCodes.Ldloc_1 &&
                        codes[i+3].opcode == OpCodes.Newobj && codes[i+4].opcode == OpCodes.Call && (MethodInfo)codes[i+4].operand == _mi_set_LookDirection) {
                            codes[i+3] = new CodeInstruction(OpCodes.Call, _mi_DuckHack);
                            codes.Insert(i+3, new CodeInstruction(OpCodes.Ldarg_0));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleLooking)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static Trilean2 DuckHack(int x, int y, LevelPlayerMotor instance) {
                if (APData.IsCurrentSlotEnabled() && !APCondition(instance) && !instance.Locked && instance.Grounded && y < 0) {
                    return new Trilean2(x, 0);
                }
                return new Trilean2(x, y);
            }
            private static bool APCondition(LevelPlayerMotor instance) {
                if (instance.player.stats.isChalice)
                    return APData.CurrentSData.playerData.dlc_cduck;
                else
                    return APData.CurrentSData.playerData.duck;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "HandleParry")]
        internal static class HandleParry {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APData.CurrentSData.playerData.parry;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "ChaliceDashParry")]
        internal static class ChaliceDashParry {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APData.CurrentSData.playerData.dlc_cparry;
            }
        }

        [HarmonyPatch(typeof(LevelPlayerMotor), "ChaliceDoubleJump")]
        internal static class ChaliceDoubleJump {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APData.CurrentSData.playerData.dlc_cdoublejump;
            }
        }
    }
}

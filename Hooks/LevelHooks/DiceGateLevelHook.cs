/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DiceGateLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(DiceGateLevel), "Start")]
        internal static class Start {
            static bool Prefix() {
                if (!Logging.IsDebugEnabled())
                    Logging.Log($"Contracts: {APClient.APSessionGSPlayerData.contracts}");
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int index = 1;
                int req_index = 0;
                int insertCount = 0;

                MethodInfo _mi_get_PlayerData_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelCompleted = typeof(PlayerData).GetMethod("CheckLevelCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_CheckLevelsCompleted = typeof(PlayerData).GetMethod("CheckLevelsCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(Start).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_PlayerData_Data &&
                        codes[i+2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i+2].operand == _mi_CheckLevelCompleted || 
                        (MethodInfo)codes[i+2].operand == _mi_CheckLevelsCompleted) && codes[i+3].opcode == OpCodes.Brfalse) {
                            bool countCondition = codes[i+1].opcode == OpCodes.Ldsfld;
                            int testCount = countCondition?APSettings.RequiredContracts[ClampReqIndex(req_index)]:index;
                            codes.Insert(i+3, new CodeInstruction(OpCodes.Ldc_I4, testCount));
                            codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            if (countCondition) req_index++;
                            else index++;
                            insertCount++;
                    }
                }
                if (insertCount!=12) throw new Exception($"{nameof(Start)}: Patch Failed! insertCount: {insertCount}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static int ClampReqIndex(int i) {
                if (i<=0) return 0;
                else if (i>=APSettings.RequiredContracts.Length) return APSettings.RequiredContracts.Length-1;
                else return i;
            }
            private static bool APCondition(bool vanillaCondition, int testCount) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.LogDebug($"Contracts: {APClient.APSessionGSPlayerData.contracts}>={testCount}");
                    return APClient.APSessionGSPlayerData.contracts>=testCount;
                }
                else return vanillaCondition;
            }
        }
    }
}
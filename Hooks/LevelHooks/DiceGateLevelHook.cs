/// Copyright 2024 JKLeckr
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
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                int index = 0;
                int req_index = 0;
                int insertCount = 0;
                MethodInfo _mi_get_PlayerData_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelCompleted = typeof(PlayerData).GetMethod("CheckLevelCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_CheckLevelsCompleted = typeof(PlayerData).GetMethod("CheckLevelsCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(Start).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_PlayerData_Data &&
                        codes[i+2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i+2].operand == _mi_CheckLevelCompleted || 
                        (MethodInfo)codes[i+2].operand == _mi_CheckLevelsCompleted) && codes[i+3].opcode == OpCodes.Brfalse) {
                            bool countCondition = codes[i+1].opcode == OpCodes.Ldsfld;
                            int testCount = countCondition?APSettings.RequiredContracts[ClampReqIndex(req_index)]-1:index;
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
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
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
                    return APClient.APSessionGSPlayerData.contracts>=testCount;
                }
                else return vanillaCondition;
            }
        }
    }
}
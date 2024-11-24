/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapEquipUIChecklistHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Init));
        }

        [HarmonyPatch(typeof(MapEquipUIChecklist), "Init")]
        internal static class Init {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                int index = 0;
                bool debug = false;

                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_CheckLevelsCompleted = typeof(PlayerData).GetMethod("CheckLevelsCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheckLevelsCompleted = typeof(Init).GetMethod("APCheckLevelsCompleted", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0; i<codes.Count-3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldsfld && codes[i+2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+2].operand == _mi_CheckLevelsCompleted && codes[i+3].opcode == OpCodes.Brtrue) {
                            List<Label> orig_labels = codes[i].labels;
                            codes[i] = new CodeInstruction(OpCodes.Ldc_I4, index) {
                                labels = orig_labels
                            };
                            codes[i+2] = new CodeInstruction(OpCodes.Call, _mi_APCheckLevelsCompleted);
                            index++;
                    }
                }
                if (index!=3) throw new Exception($"{nameof(Init)}: Patch Failed! {index}");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APCheckLevelsCompleted(int index, Levels[] levels) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.LogDebug($"{index}: {APData.CurrentSData.playerData.contracts}>={APSettings.RequiredContracts[index]}");
                    return APSettings.ShowUnaccessibleIslesInList || APData.CurrentSData.playerData.contracts >= APSettings.RequiredContracts[index];
                }
                return !PlayerData.Data.CheckLevelsCompleted(levels);
            }
        }
    }
}

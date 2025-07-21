/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapEquipUIChecklistHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(UpdateList));
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
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i + 1].opcode == OpCodes.Ldsfld && codes[i + 2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i + 2].operand == _mi_CheckLevelsCompleted && codes[i + 3].opcode == OpCodes.Brtrue) {
                        List<Label> orig_labels = codes[i].labels;
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4, index) {
                            labels = orig_labels
                        };
                        codes[i + 2] = new CodeInstruction(OpCodes.Call, _mi_APCheckLevelsCompleted);
                        index++;
                    }
                }
                if (index != 3) throw new Exception($"{nameof(Init)}: Patch Failed! {index}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCheckLevelsCompleted(int index, Levels[] levels) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.LogDebug($"{index}: {APData.CurrentSData.playerData.contracts}>={APSettings.RequiredContracts[index]}");
                    return APSettings.ShowUnaccessibleIslesInList || APData.CurrentSData.playerData.contracts >= APSettings.RequiredContracts[index];
                }
                return PlayerData.Data.CheckLevelsCompleted(levels);
            }
        }

        [HarmonyPatch(typeof(MapEquipUIChecklist), "UpdateList")]
        internal static class UpdateList {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                Type listType = typeof(List<>).MakeGenericType(typeof(Levels));
                MethodInfo _mi_GetIEnumerator = typeof(List<Levels>).GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_MapLevelList = typeof(UpdateList).GetMethod("MapLevelList", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Br && codes[i + 1].opcode == OpCodes.Ldloc_0 && codes[i + 2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i + 2].operand == _mi_GetIEnumerator && codes[i + 3].opcode == OpCodes.Stloc_S
                    ) {
                        if (success) throw new Exception($"{nameof(UpdateList)}: Patch condition is true more than once!");
                        CodeInstruction[] ncodes = [
                            new(OpCodes.Ldloc_0),
                            new CodeInstruction(OpCodes.Call, _mi_MapLevelList),
                        ];
                        codes.InsertRange(i + 1, ncodes);
                        success = true;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateList)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool IsLabelInCI(Label label, CodeInstruction instruction) {
                foreach (Label l in instruction.labels) {
                    if (l == label) return true;
                }
                return false;
            }
            private static void MapLevelList(List<Levels> levelList) {
                if (APData.IsCurrentSlotEnabled()) {
                    for (int i = 0; i < levelList.Count; i++) {
                        levelList[i] = LevelMap.GetMappedLevel(levelList[i], true);
                        Logging.LogDebug($"list: {i}: {levelList[i]}"); // FIXME: List does not map!
                    }
                }
            }
        }
    }
}

/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class KitchenLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(CheckIfBossesCompleted));
            Harmony.CreateAndPatchAll(typeof(OnLevelStart));
        }

        internal const int DIALOGUER_ID = 23;
        internal static bool firstVisit = false;

        [HarmonyPatch(typeof(KitchenLevel), "CheckIfBossesCompleted")]
        internal static class CheckIfBossesCompleted {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_worldDLCBossLevels = typeof(Level).GetField("worldDLCBossLevels", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelsHaveMinDifficulty = typeof(PlayerData).GetMethod("CheckLevelsHaveMinDifficulty", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(CheckIfBossesCompleted).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i+1].operand == _fi_worldDLCBossLevels &&
                        codes[i+2].opcode == OpCodes.Ldc_I4_1 && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_CheckLevelsHaveMinDifficulty && codes[i+4].opcode == OpCodes.Brfalse) {
                            codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(CheckIfBossesCompleted)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(bool orig) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"Ingredients: {APClient.APSessionGSPlayerData.dlc_ingredients} >= {APSettings.DLCRequiredIngredients}");
                    return APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCRequiredIngredients;
                } else return orig;
            }
        }

        [HarmonyPatch(typeof(KitchenLevel), "OnLevelStart")]
        internal static class OnLevelStart {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_GetGlobalFloat = typeof(Dialoguer).GetMethod(
                    "GetGlobalFloat",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(int)],
                    null
                );
                MethodInfo _mi_APCondition = typeof(OnLevelStart).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == DIALOGUER_ID && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_GetGlobalFloat &&
                        codes[i+2].opcode == OpCodes.Ldc_R4 && (float)codes[i+2].operand == 1 && codes[i+3].opcode == OpCodes.Bne_Un) {
                            codes[i+3].opcode = OpCodes.Brfalse;
                            codes.Insert(i+3, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(OnLevelStart)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(float a, float b) {
                bool orig = a == b;
                if (APData.IsCurrentSlotEnabled()) {
                    return orig && !firstVisit;
                } else return orig;
            }
        }
    }
}

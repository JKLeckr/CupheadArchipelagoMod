/// Copyright 2025 JKLeckr
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
        }

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
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
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
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
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
    }
}

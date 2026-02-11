/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapDLCHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(CheckIfBossesCompleted));
        }

        [HarmonyPatch(typeof(MapDLC), "CheckIfBossesCompleted")]
        internal static class CheckIfBossesCompleted {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_CheckLevelsHaveMinDifficulty =
                    typeof(PlayerData).GetMethod("CheckLevelsHaveMinDifficulty", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(CheckIfBossesCompleted).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-1;i++) {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == _mi_CheckLevelsHaveMinDifficulty && codes[i+1].opcode == OpCodes.Brfalse) {
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(CheckIfBossesCompleted)}: Patch Failed!");
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(bool orig) {
                if (APData.IsCurrentSlotEnabled()) {
                    return APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCRequiredIngredients;
                } else return orig;
            }
        }
    }
}

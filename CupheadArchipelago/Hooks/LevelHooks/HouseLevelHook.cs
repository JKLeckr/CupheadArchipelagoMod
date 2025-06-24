/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class HouseLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(HouseLevel), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_dialoguerVariableID = typeof(HouseLevel).GetField("dialoguerVariableID", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_base_Start = typeof(Level).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_APSetup = typeof(Start).GetMethod("APSetup", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0; i<codes.Count-1; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_base_Start) {
                        CodeInstruction[] ncodes = [
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, _fi_dialoguerVariableID),
                            new CodeInstruction(OpCodes.Call, _mi_APSetup),
                        ];
                        codes.InsertRange(i+2, ncodes);
                        i += ncodes.Length;
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Start)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void APSetup(int dialoguerVarId) {
                if (Config.IsSkippingCutscene(Cutscenes.KettleIntro)) {
                    Dialoguer.SetGlobalFloat(dialoguerVarId, 1f);
                }
            }
        }
    }
}

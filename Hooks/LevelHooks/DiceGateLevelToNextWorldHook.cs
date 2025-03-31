/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DiceGateLevelToNextWorldHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(go_cr));
        }

        [HarmonyPatch(typeof(DiceGateLevelToNextWorld), "go_cr", MethodType.Enumerator)]
        internal static class go_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                FieldInfo _fi_sessionStarted = typeof(PlayerData.MapData).GetField("sessionStarted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_GetMapData = typeof(PlayerData).GetMethod("GetMapData", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_IsSkippingCutscene = typeof(go_cr).GetMethod("IsSkippingCutscene", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+2].operand == _mi_GetMapData &&
                        codes[i+3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+3].operand == _fi_sessionStarted && codes[i+4].opcode == OpCodes.Brfalse) {
                            codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_IsSkippingCutscene));
                            i+=5;
                            success++;
                    }
                }
                if (success!=2) throw new Exception($"{nameof(go_cr)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool IsSkippingCutscene(bool origCond) {
                return origCond || Config.IsSkippingCutscene(Cutscenes.DieHouseCutscenes);
            }
        }
    }
}
/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Util;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class AirplaneLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(handle_secret_intro_cr));
        }

        [HarmonyPatch(typeof(AirplaneLevel), "handle_secret_intro_cr", MethodType.Enumerator)]
        internal static class handle_secret_intro_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                Type crtype = Reflection.GetEnumeratorType(
                    typeof(AirplaneLevel).GetMethod("handle_secret_intro_cr", BindingFlags.NonPublic | BindingFlags.Instance)
                );
                FieldInfo _fi_PC = crtype.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_secretTriggered = typeof(Level).GetField("secretTriggered", BindingFlags.NonPublic | BindingFlags.Instance);

                MethodInfo _mi_APDebug = typeof(handle_secret_intro_cr).GetMethod("APDebug", BindingFlags.NonPublic | BindingFlags.Static);

                Logging.LogDebug(_mi_APDebug);

                Label l_0 = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                if (codes[0].opcode == OpCodes.Ldarg_0 && codes[1].opcode == OpCodes.Ldfld && (FieldInfo)codes[1].operand == _fi_PC &&
                    codes[2].opcode == OpCodes.Stloc_0 && codes[3].opcode == OpCodes.Ldarg_0 && codes[4].opcode == OpCodes.Ldc_I4_M1 &&
                    codes[5].opcode == OpCodes.Stfld && (FieldInfo)codes[5].operand == _fi_PC && codes[6].opcode == OpCodes.Ldloc_0 &&
                    codes[7].opcode == OpCodes.Switch && codes[8].opcode == OpCodes.Br) {
                        List<CodeInstruction> ncodes = [
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldc_I4_1),
                            new CodeInstruction(OpCodes.Stfld, _fi_secretTriggered),
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, _fi_secretTriggered),
                            new CodeInstruction(OpCodes.Call, _mi_APDebug)
                        ];
                        ncodes[0].labels.Add(l_0);
                        ((Label[])codes[7].operand)[0] = l_0;
                        codes.InsertRange(9, ncodes);
                        success = true;
                }
                if (!success) throw new Exception($"{nameof(handle_secret_intro_cr)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }
            
                return codes;
            }

            private static void APDebug(object msg) {
                Logging.Log(msg);
            }
        }
    }
}

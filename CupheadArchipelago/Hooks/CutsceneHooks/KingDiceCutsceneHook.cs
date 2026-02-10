/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.CutsceneHooks {
    internal class KingDiceCutsceneHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(KingDiceCutscene), "Awake")]
        internal static class Awake {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelsHaveMinDifficulty = typeof(PlayerData).GetMethod("CheckLevelsHaveMinDifficulty", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_have_all_contracts_cr = typeof(KingDiceCutscene).GetMethod("have_all_contracts_cr", BindingFlags.NonPublic | BindingFlags.Instance);

                Label lallgood = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-5;i++) {
                    if ((success&1)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldsfld &&
                        codes[i+2].opcode == OpCodes.Ldc_I4_1 && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_CheckLevelsHaveMinDifficulty &&
                        codes[i+4].opcode == OpCodes.Brfalse) {
                            Label lorig_if = il.DefineLabel();
                            Label lorig_false = (Label)codes[i+4].operand;
                            codes[i].labels.Add(lorig_if);
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brfalse, lorig_if),
                                CodeInstruction.Call(() => APGotRequireContracts()),
                                new CodeInstruction(OpCodes.Brfalse, lorig_false),
                                new CodeInstruction(OpCodes.Br, lallgood)
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Length;
                            success |= 1;
                    }
                    if ((success&2)==0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldarg_0 && codes[i+2].opcode == OpCodes.Call &&
                        (MethodInfo)codes[i+2].operand == _mi_have_all_contracts_cr && codes[i+3].opcode == OpCodes.Call && codes[i+4].opcode == OpCodes.Pop &&
                        codes[i+5].opcode == OpCodes.Br) {
                            codes[i].labels.Add(lallgood);
                            success |= 2;
                    }
                    if (success==3) break;
                }
                if (success!=3) throw new Exception($"{nameof(Awake)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APGotRequireContracts() {
                int cindex = APSettings.RequiredContracts.Length-1; // Should be 2.
                Logging.Log($"Contracts: {APClient.APSessionGSPlayerData.contracts}>={APSettings.RequiredContracts[cindex]}");
                return APClient.APSessionGSPlayerData.contracts >= APSettings.RequiredContracts[cindex];
            }
        }
    }
}

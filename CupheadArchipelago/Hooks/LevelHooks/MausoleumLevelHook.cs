/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class MausoleumLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(OnWin));
        }

        [HarmonyPatch(typeof(MausoleumLevel), "Awake")]
        internal static class Awake {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int insertCount = 0;

                MethodInfo _mi_IsCurrentSlotEnabled = typeof(APData).GetMethod("IsCurrentSlotEnabled", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_IsChecked = typeof(Awake).GetMethod("IsChecked", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_get_PlayerData_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_get_mode = typeof(Level).GetProperty("mode", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_IsUnlocked = typeof(PlayerData).GetMethod(
                    "IsUnlocked", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {typeof(PlayerId), typeof(Super)}, null);
                FieldInfo _fi_noChalice = typeof(MausoleumLevel).GetField("noChalice", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_base_Awake = typeof(Level).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

                Label vanilla_label = il.DefineLabel();
                Label tgt_label = il.DefineLabel();
                Label after_label = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_PlayerData_Data && 
                        codes[i+1].opcode == OpCodes.Ldc_I4 && (int)codes[i+1].operand == (int)PlayerId.Any && 
                        codes[i+2].opcode == OpCodes.Ldc_I4 && (int)codes[i+2].operand == (int)Super.level_super_beam && 
                        codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_IsUnlocked) {
                            codes[i].labels.Add(vanilla_label);
                            codes.Insert(i, new CodeInstruction(OpCodes.Call, _mi_IsCurrentSlotEnabled));
                            codes.Insert(i+1, new CodeInstruction(OpCodes.Brfalse, vanilla_label));
                            codes.Insert(i+2, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i+3, new CodeInstruction(OpCodes.Call, _mi_get_mode));
                            codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_IsChecked));
                            codes.Insert(i+5, new CodeInstruction(OpCodes.Brtrue, tgt_label));
                            codes.Insert(i+6, new CodeInstruction(OpCodes.Br, after_label));
                            i+=7;
                            insertCount++;
                    }
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldc_I4_1 && 
                        codes[i+2].opcode == OpCodes.Stfld && (FieldInfo)codes[i+2].operand == _fi_noChalice) {
                            codes[i].labels.Add(tgt_label);
                            insertCount++;
                    }
                    if (debug) {
                        Logging.Log(i);
                        for (int j=0;j<4;j++) {
                            Logging.Log($"{codes[i+j].opcode}: {codes[i+j].operand}");
                        }
                        Logging.Log(codes[i+1].opcode == OpCodes.Ldarg_0);
                        Logging.Log(codes[i+2].opcode == OpCodes.Call);
                        Logging.Log(codes[i+2].opcode == OpCodes.Call && (MethodInfo)codes[i+2].operand == _mi_base_Awake);
                        Logging.Log(codes[i+3].opcode == OpCodes.Ret);
                        Logging.Log("-");
                    }
                    if (codes[i+1].opcode == OpCodes.Ldarg_0 && codes[i+2].opcode == OpCodes.Call &&
                        (MethodInfo)codes[i+2].operand == _mi_base_Awake && codes[i+3].opcode == OpCodes.Ret) {
                            codes[i+1].labels.Add(after_label);
                            insertCount++;
                    }
                }
                if (insertCount!=3) throw new Exception($"{nameof(Awake)}: Patch Failed! insertCount: {insertCount}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool IsChecked(Level.Mode mode) {
                if (!PlayerData.Data.GetLevelData(Levels.Mausoleum).completed) return false;
                return mode switch {
                    Level.Mode.Normal => APClient.IsLocationChecked(APLocation.level_mausoleum_ii),
                    Level.Mode.Hard => APClient.IsLocationChecked(APLocation.level_mausoleum_iii),
                    _ => APClient.IsLocationChecked(APLocation.level_mausoleum_i),// Level.Mode.Easy
                };
            }
        }

        [HarmonyPatch(typeof(MausoleumLevel), "OnWin")]
        internal static class OnWin {
            static bool Prefix() => !APData.IsCurrentSlotEnabled();
        }
    }
}

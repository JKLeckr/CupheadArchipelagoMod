/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCHookBase {
        internal static class MapNPCCoinHookBase {
            internal static IEnumerable<CodeInstruction> MapNPCCoinHookTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, long loc, bool debug = false) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;

                FieldInfo _fi_coinManager = typeof(PlayerData).GetField("coinManager", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_GetCoinCollected = 
                    typeof(PlayerData.PlayerCoinManager).GetMethod("GetCoinCollected", BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null);
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_Current  = typeof(MapEventNotification).GetProperty("Current", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_ShowEvent = typeof(MapEventNotification).GetMethod("ShowEvent", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCoinCondition = typeof(MapNPCCoinHookBase).GetMethod("APCoinCondition", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_IsAPEnabled = typeof(MapNPCCoinHookBase).GetMethod("IsAPEnabled", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APCheck = typeof(MapNPCCoinHookBase).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_afterac = il.DefineLabel();
                Label l_end = il.DefineLabel();

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (i < codes.Count-8) {
                        if ((success&1)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldfld &&
                            (FieldInfo)codes[i+1].operand == _fi_coinManager && codes[i+2].opcode == OpCodes.Ldarg_0 && codes[i+3].opcode == OpCodes.Ldfld &&
                            codes[i+4].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+4].operand == _mi_GetCoinCollected && codes[i+5].opcode == OpCodes.Brtrue) {
                                List<CodeInstruction> ncodes = [
                                    new CodeInstruction(OpCodes.Ldc_I8, loc),
                                    new CodeInstruction(OpCodes.Call, _mi_APCoinCondition),
                                ];
                                codes.InsertRange(i+5, ncodes);
                                i+=ncodes.Count;
                                success |= 1;
                        }
                        if ((success&2)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+3].opcode == OpCodes.Callvirt &&
                            (MethodInfo)codes[i+3].operand == _mi_AddCurrency && codes[i+4].opcode == OpCodes.Call && (MethodInfo)codes[i+4].operand == _mi_get_Data && codes[i+7].opcode == OpCodes.Callvirt &&
                            (MethodInfo)codes[i+7].operand == _mi_AddCurrency) {
                                List<CodeInstruction> ncodes = [
                                    new CodeInstruction(OpCodes.Ldc_I8, loc),
                                    new CodeInstruction(OpCodes.Call, _mi_APCheck),
                                ];
                                codes.InsertRange(i+8, ncodes);
                                codes[i+8].labels.Add(l_afterac);
                                List<CodeInstruction> ncodes2 = [
                                    new CodeInstruction(OpCodes.Call, _mi_IsAPEnabled),
                                    new CodeInstruction(OpCodes.Brtrue, l_end),
                                ];
                                codes.InsertRange(i, ncodes2);
                                i+=ncodes2.Count;
                                success |= 2;
                        }
                    }
                    if ((success&4)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Current && codes[i+2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+2].operand == _mi_ShowEvent) {
                            codes[i+3].labels.Add(l_end);
                            List<CodeInstruction> ncodes = [
                                new CodeInstruction(OpCodes.Call, _mi_IsAPEnabled),
                                new CodeInstruction(OpCodes.Brtrue, l_end),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Count;
                            success |= 4;
                    }
                }
                if (success!=7) throw new Exception($"{nameof(MapNPCCoinHookTranspiler)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }
            private static bool APCoinCondition(bool orig, long locationId) {
                if (APData.IsCurrentSlotEnabled()) {
                    return !APClient.IsLocationChecked(locationId);
                }
                return orig;
            }
            private static bool IsAPEnabled() => APData.IsCurrentSlotEnabled();
            private static void APCheck(long loc) {
                if (!APClient.IsLocationChecked(loc))
                    APClient.Check(loc);
            }
            private static bool pc(bool cond) {
                Logging.Log($"C: {cond}");
                return cond;
            }
        }
    }
}

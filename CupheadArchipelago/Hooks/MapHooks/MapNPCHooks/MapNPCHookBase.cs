/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

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
                MethodInfo _mi_APCoinCheck = typeof(MapNPCCoinHookBase).GetMethod("APCoinCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_afterac = il.DefineLabel();
                Label l_end = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (i < codes.Count-8) {
                        if ((success&1)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldfld &&
                            (FieldInfo)codes[i+1].operand == _fi_coinManager && codes[i+2].opcode == OpCodes.Ldarg_0 && codes[i+3].opcode == OpCodes.Ldfld &&
                            codes[i+4].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+4].operand == _mi_GetCoinCollected && codes[i+5].opcode == OpCodes.Brtrue) {
                                CodeInstruction[] ncodes = [
                                    new CodeInstruction(OpCodes.Ldc_I8, loc),
                                    new CodeInstruction(OpCodes.Call, _mi_APCoinCondition),
                                ];
                                codes.InsertRange(i+5, ncodes);
                                i+=ncodes.Length;
                                success |= 1;
                        }
                        if ((success&2)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+3].opcode == OpCodes.Callvirt &&
                            (MethodInfo)codes[i+3].operand == _mi_AddCurrency && codes[i+4].opcode == OpCodes.Call && (MethodInfo)codes[i+4].operand == _mi_get_Data && codes[i+7].opcode == OpCodes.Callvirt &&
                            (MethodInfo)codes[i+7].operand == _mi_AddCurrency) {
                                codes[i+8].labels.Add(l_afterac);
                                CodeInstruction[] ncodes = [
                                    new CodeInstruction(OpCodes.Ldc_I8, loc),
                                    new CodeInstruction(OpCodes.Call, _mi_APCoinCheck),
                                    new CodeInstruction(OpCodes.Brtrue, l_afterac),
                                ];
                                codes.InsertRange(i, ncodes);
                                i+=ncodes.Length;
                                success |= 2;
                        }
                    }
                    if ((success&4)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Current && codes[i+2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+2].operand == _mi_ShowEvent) {
                            codes[i+3].labels.Add(l_end);
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Call, _mi_IsAPEnabled),
                                new CodeInstruction(OpCodes.Brtrue, l_end),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Length;
                            success |= 4;
                    }
                }
                if (success!=7) throw new Exception($"{nameof(MapNPCCoinHookTranspiler)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCoinCondition(bool orig, long locationId) {
                Logging.Log($"APCoinCondition: {orig}, {locationId}");
                if (APData.IsCurrentSlotEnabled()) {
                    return APClient.IsLocationChecked(locationId);
                }
                return orig;
            }
            private static bool APCoinCheck(long locationId) {
                Logging.Log($"APCoinCheck: {locationId}");
                if (APData.IsCurrentSlotEnabled()) {
                    APCheck(locationId);
                    return true;
                }
                return false;
            }
            private static bool IsAPEnabled() => APData.IsCurrentSlotEnabled();
            private static void APCheck(long loc) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (!APClient.IsLocationChecked(loc))
                        APClient.Check(loc);
                }
            }
        }
        internal static class MapNPCQuestHookBase {
            internal static IEnumerable<CodeInstruction> MapNPCQuestHookTranspiler(IEnumerable<CodeInstruction> instructions, long loc, bool debug = false) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;

                MethodInfo _mi_SaveCurrentFile = typeof(PlayerData).GetMethod("SaveCurrentFile", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Current = typeof(MapUI).GetProperty("Current", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_Refresh = typeof(MapUI).GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(MapNPCCoinHookBase).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-2;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_SaveCurrentFile && codes[i+1].opcode == OpCodes.Call &&
                        (MethodInfo)codes[i+1].operand == _mi_get_Current && codes[i+2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+2].operand == _mi_Refresh) {
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Ldc_I8, loc),
                                new CodeInstruction(OpCodes.Call, _mi_APCheck),
                            ];
                            codes.InsertRange(i, ncodes);
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(MapNPCQuestHookBase)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            private static void APCheck(long loc) {
                if (APData.IsCurrentSlotEnabled() && !APClient.IsLocationChecked(loc))
                    APClient.Check(loc);
            }
        }
    }
}

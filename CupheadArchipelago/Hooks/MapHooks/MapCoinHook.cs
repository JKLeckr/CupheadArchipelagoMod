/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapCoinHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnTriggerEnter2D));
        }

        [HarmonyPatch(typeof(MapCoin), "Start")]
        internal static class Start {
            /*static bool Prefix(MapCoin __instance) {
                Logging.Log("Coin: "+__instance.coinID);
                return false;
            }*/
            static void Postfix(MapCoin __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (APClient.IsLocationChecked(CoinIdMap.GetAPLocation(__instance.coinID))) {
                        Logging.LogDebug("[MapCoin] Already checked, bye.");
                        PlayerData.Data.coinManager.SetCoinValue(__instance.coinID, true, PlayerId.PlayerOne);
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapCoin), "OnTriggerEnter2D")]
        internal static class OnTriggerEnter2D {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_coinID = typeof(MapCoin).GetField("coinID", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_SaveCurrentFile = typeof(PlayerData).GetMethod("SaveCurrentFile", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(OnTriggerEnter2D).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label savepoint = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if ((success&1) == 0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldc_I4_0 &&
                        codes[i+2].opcode == OpCodes.Ldc_I4_1 && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_AddCurrency) {
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi_coinID),
                                new CodeInstruction(OpCodes.Call, _mi_APCheck),
                                new CodeInstruction(OpCodes.Brtrue, savepoint),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Length;
                            success |= 1;
                    }
                    if ((success&2) == 0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_SaveCurrentFile) {
                        codes[i].labels.Add(savepoint);
                        success |= 2;
                    }
                }
                if (success!=3) throw new Exception($"{nameof(OnTriggerEnter2D)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCheck(string coinID) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"Check: {coinID}");
                    APClient.Check(CoinIdMap.GetAPLocation(coinID));
                    return true;
                } else return false;
            }
        }
    }
}

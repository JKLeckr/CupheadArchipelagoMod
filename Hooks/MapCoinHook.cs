/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks {
    internal class MapCoinHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start)); //Debug
        }

        [HarmonyPatch(typeof(MapCoin), "Start")]
        internal static class Start {
            static bool Prefix(string ___coinID, MapCoin __instance) {
                Plugin.Log("Coin: "+___coinID);
                return false;
            }
        }

        [HarmonyPatch(typeof(MapCoin), "OnTriggerEnter2D")]
        internal static class OnTriggerEnter2D {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                FieldInfo _fi_coinID = typeof(MapCoin).GetField("coinID", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_SaveCurrentFile = typeof(PlayerData).GetMethod("SaveCurrentFile", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(OnTriggerEnter2D).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                bool jumpPointFound = false;
                int insertPoint = -1;
                Label jumpPoint = il.DefineLabel();
                for (int i=0;i<codes.Count-4;i++) {
                    if (!jumpPointFound && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_SaveCurrentFile) {
                        codes[i].labels.Add(jumpPoint);
                        jumpPointFound = true;
                    }
                    if (insertPoint<0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_AddCurrency) {
                        insertPoint = i;
                    }
                }
                
                if (insertPoint >= 0 && jumpPointFound) {
                    codes.Insert(insertPoint, new CodeInstruction(OpCodes.Ldfld, _fi_coinID));
                    codes.Insert(insertPoint+1, new CodeInstruction(OpCodes.Call, _mi_APCheck));
                    codes.Insert(insertPoint+2, new CodeInstruction(OpCodes.Brtrue_S, jumpPoint));
                    //codes.Insert(insertPoint+2, new CodeInstruction(OpCodes.Ret));
                    //Plugin.Log("[MapCoinHook] Successfully Patched OnDialoguerMessageEvent");
                    Plugin.Log("[MapCoinHook] Successfully Patched OnDialoguerMessageEvent", LoggingFlags.Transpiler, LogLevel.Debug);
                }
                else {
                    Plugin.Log("[MapCoinHook] Cannot find insertion point", LogLevel.Error);
                }
                
                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log($"{code.opcode}: {code.operand}");
                }*/

                return codes;
            }

            private static bool APCheck(string coinID) {
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log($"Check: {coinID}");
                    APClient.Check(CoinIdMap.GetAPLocation(coinID));
                    return true;
                } else return false;
            }
        }
    }
}
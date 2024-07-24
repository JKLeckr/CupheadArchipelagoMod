/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks {
    internal class MapNPCAppletravellerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        [HarmonyPatch(typeof(MapNPCAppletraveller), "Start")]
        internal static class Start {
            static void Postfix(string ___coinID1, string ___coinID2, string ___coinID3) {
                if (APData.IsCurrentSlotEnabled()) {
                    PlayerData.PlayerCoinManager coinManager = PlayerData.Data.coinManager;
                    bool collected = coinManager.GetCoinCollected(___coinID1);
                    bool ap_checked = APClient.IsLocationChecked(APLocation.npc_mac);
                    if (!collected && ap_checked) {
                        coinManager.SetCoinValue(___coinID1, true, PlayerId.Any);
                        coinManager.SetCoinValue(___coinID2, true, PlayerId.Any);
                        coinManager.SetCoinValue(___coinID3, true, PlayerId.Any);
                    }
                    else if (collected && !ap_checked) {
                        APCheck();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapNPCAppletraveller), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                MethodInfo _mi_SaveCurrentFile = typeof(PlayerData).GetMethod("SaveCurrentFile", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);

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
                    codes.Insert(insertPoint, CodeInstruction.Call(() => APCheck()));
                    codes.Insert(insertPoint+1, new CodeInstruction(OpCodes.Brtrue_S, jumpPoint));
                    //codes.Insert(insertPoint+2, new CodeInstruction(OpCodes.Ret));
                    //Plugin.Log("[MapNPCAppletravellerHook] Successfully Patched OnDialoguerMessageEvent");
                    Plugin.Log("[MapNPCAppletravellerHook] Successfully Patched OnDialoguerMessageEvent", LoggingFlags.Transpiler, LogLevel.Debug);
                }
                else {
                    Plugin.Log("[MapNPCAppletravellerHook] Cannot find insertion point", LogLevel.Error);
                }
                
                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log($"{code.opcode}: {code.operand}");
                }*/

                return codes;
            }
        }

        private static bool APCheck() {
            if (APData.IsCurrentSlotEnabled()) {
                Plugin.Log("Check: mac");
                APClient.Check(APLocation.npc_mac);
                return true;
            } else return false;
        }
    }
}
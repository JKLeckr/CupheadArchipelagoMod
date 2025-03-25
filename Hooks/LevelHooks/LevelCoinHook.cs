/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class LevelCoinHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Collect));
        }

        [HarmonyPatch(typeof(LevelCoin), "Awake")]
        internal static class Awake {
            // DEBUG
            /*static bool Prefix(LevelCoin __instance) {
                Vector3 pos = __instance.transform.position;
                Logging.Log("Coin: "+pos.x+", "+pos.y+" : "+__instance.GlobalID);
                return true;
            }*/
            
            static void Postfix(LevelCoin __instance, ref bool ____collected) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (APClient.IsLocationChecked(CoinIdMap.GetAPLocation(__instance.GlobalID))) {
                        PlayerData.Data.coinManager.SetCoinValue(__instance.GlobalID, true, PlayerId.PlayerOne);
                        ____collected = true;
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelCoin), "Collect")]
        internal static class Collect {
            static bool Prefix(LevelCoin __instance) {
                if (APData.IsCurrentSlotEnabled())
                    Logging.Log($"Coin Collected: {APClient.GetCheck(CoinIdMap.GetAPLocation(__instance.GlobalID)).LocationName}");
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;
                
                FieldInfo _fi__collected = typeof(LevelCoin).GetField("_collected", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo _mi_APCheck = typeof(Collect).GetMethod("APCheck", BindingFlags.Static | BindingFlags.NonPublic);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldc_I4_1 &&
                        codes[i+2].opcode == OpCodes.Stfld && (FieldInfo)codes[i+2].operand == _fi__collected) {
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Ldarg_1));
                        codes.Insert(i+2, new CodeInstruction(OpCodes.Call, _mi_APCheck));
                        codes.Insert(i+3, new CodeInstruction(OpCodes.Pop));
                        success = true;
                        break;
                    }
                }
                if (!success) {
                    throw new Exception("[LevelCoinHook] Failed to Patch Collect");
                }
                if (debug) {
                    Logging.Log($"===");
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static bool APCheck(LevelCoin instance, PlayerId player) {
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.Check(CoinIdMap.GetAPLocation(instance.GlobalID));
                    PlayerData.Data.coinManager.SetCoinValue(instance.GlobalID, true, player);
                    return true;
                }
                return false;
            }
        }
    }
}
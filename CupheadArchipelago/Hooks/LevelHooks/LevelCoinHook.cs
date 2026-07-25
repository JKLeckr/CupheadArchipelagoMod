/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

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

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_GetCoinCollected = typeof(PlayerData).GetMethod("GetCoinCollected", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(Awake).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                        codes[i + 2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 2].operand == _mi_GetCoinCollected && codes[i + 3].opcode == OpCodes.Brfalse
                    ) {
                        List<CodeInstruction> ncodes = [
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Call, _mi_APCondition)
                        ];

                        codes.InsertRange(i + 3, ncodes);

                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Awake)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            static void Postfix(LevelCoin __instance, bool ____collected) {
                Logging.Log($"{CoinIdMap.GetAPLocation(__instance.GlobalID)} got: {____collected}");
            }

            private static bool APCondition(bool orig, LevelCoin instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (APClient.IsLocationChecked(CoinIdMap.GetAPLocation(instance.GlobalID))) {
                        Logging.LogDebug($"{instance.GlobalID} {GetLocationId(instance.GlobalID)} already Collected");
                        PlayerData.Data.coinManager.SetCoinValue(instance.GlobalID, true, PlayerId.PlayerOne);
                        return true;
                    }
                    return false;
                }
                return orig;
            }
            private static long GetLocationId(string coinId) {
                if (CoinIdMap.CoinIDExists(coinId)) {
                    return CoinIdMap.GetAPLocation(coinId);
                } return -1;
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
                    Dbg.LogCodeInstructions(codes);
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
                    Logging.Log($"---");
                    Dbg.LogCodeInstructions(codes);
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

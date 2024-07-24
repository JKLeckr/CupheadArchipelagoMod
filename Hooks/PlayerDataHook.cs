/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Reflection;
using HarmonyLib;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CupheadArchipelago.Hooks {
    public class PlayerDataHook {
        public static void Hook() {
            APData.Init();
            //Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(OnLoaded));
            Harmony.CreateAndPatchAll(typeof(SaveCurrentFile));
            Harmony.CreateAndPatchAll(typeof(AddCurrency));
            Harmony.CreateAndPatchAll(typeof(ApplyLevelCoins));
            //PlayerCoinManagerHook.Hook();
        }

        [HarmonyPatch(typeof(PlayerData), "OnLoaded")]
        internal static class OnLoaded {
            static void Postfix() {
                Plugin.Log("Loading...");
                APData.LoadData();
            }
        }

        [HarmonyPatch(typeof(PlayerData), "SaveCurrentFile")]
        internal static class SaveCurrentFile {
            static void Postfix() {
                int index = PlayerData.CurrentSaveFileIndex;
                Debug.Log($"[APData] Saving to slot {index}");
                APData.Save(index);
            }
        }

        [HarmonyPatch(typeof(PlayerData), "AddCurrency")]
        internal static class AddCurrency {
            static bool Prefix() {
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log("[AddCurrency] Disabled.");
                    return false;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "ApplyLevelCoins")]
        internal static class ApplyLevelCoins {
            static bool Prefix(PlayerData.PlayerCoinManager ___coinManager, ref PlayerData.PlayerCoinManager ___levelCoinManager) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (APSettings.CoinChecksVanilla) {
                        foreach (PlayerData.PlayerCoinProperties coin in ___levelCoinManager.coins) {
                            Plugin.Log("Got Coin {0}", coin.coinID);
                            APClient.Check(CoinIdMap.GetAPLocation(coin.coinID));
                            ___coinManager.SetCoinValue(coin.coinID, coin.collected, coin.player);
		                }
                    } else {
                        Plugin.Log("[ApplyLevelCoins] Disabled.");
                    }
                    ___levelCoinManager = new PlayerData.PlayerCoinManager();
                    return false;
                } else return true;
            }
        }

        internal static class PlayerCoinManagerHook {
            internal static void Hook() {
                Harmony.CreateAndPatchAll(typeof(GetCoinCollected));
            }

            [HarmonyPatch(typeof(PlayerData.PlayerCoinManager), "GetCoinCollected", new Type[] {typeof(string)})]
            internal static class GetCoinCollected {
                private static MethodInfo _mi_GetCoin__str;

                static GetCoinCollected() {
                    _mi_GetCoin__str = typeof(PlayerData.PlayerCoinManager).GetMethod("GetCoin",
                        BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]{typeof(string)}, null);
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    bool found = false;

                    for (int i=0;i<codes.Count-1;i++) {
                        if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_GetCoin__str) {
                            codes[i] = CodeInstruction.Call(typeof(GetCoinCollected), "APGetCoinCollected");
                            codes.RemoveAt(i+1);
                            found = true;
                            break;
                        }
                    }
                    if (found) Plugin.Log("[GetCoinCollected] Successfully patched", LoggingFlags.Transpiler);

                    /*foreach (CodeInstruction code in codes)
                        Console.WriteLine(code);*/

                    return codes;
                }

                private static bool APGetCoinCollected(PlayerData.PlayerCoinManager instance, string coinID) {
                    return ((PlayerData.PlayerCoinProperties)_mi_GetCoin__str.Invoke(instance, new object[]{coinID})).collected;
                }
            }
        }
    }
}
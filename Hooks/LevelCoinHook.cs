/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks {
    internal class LevelCoinHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Collect));
        }

        [HarmonyPatch(typeof(LevelCoin), "Awake")]
        internal static class Awake {
            // DEBUG
            static bool Prefix(LevelCoin __instance, ref SpriteRenderer ____spriteRenderer, ref bool ____collected) {
		        ____spriteRenderer = __instance.GetComponent<SpriteRenderer>();
                Vector3 pos = __instance.transform.position;
                Plugin.Log("Coin: "+pos.x+", "+pos.y+" : "+__instance.GlobalID);
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelCoin), "Collect")]
        internal static class Collect {
            static bool Prefix(LevelCoin __instance, ref bool ____collected) {
                Plugin.Log($"Coin Collected: {CoinIdMap.GetAPLocation(__instance.GlobalID).Name}");
                return true;
            }
            // TODO: Add Transpiler for collecting checks on coin grab instead of vanilla behavior at the end of level. This will be an option in Archipelago

            private static bool APCheck(LevelCoin instance, PlayerId player) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (!APSettings.CoinChecksVanilla) {
                        APClient.Check(CoinIdMap.GetAPLocation(instance.GlobalID));
                        PlayerData.Data.coinManager.SetCoinValue(instance.GlobalID, true, player);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
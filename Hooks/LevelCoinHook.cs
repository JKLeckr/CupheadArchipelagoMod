using System;
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
                Plugin.Log("Coin Collected: {0}", CoinIdMap.GetAPLocationId(__instance.GlobalID));
                return true;
            }
        }
    }
}
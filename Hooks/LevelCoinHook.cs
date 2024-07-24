/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks {
    internal class LevelCoinHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Collect));
        }

        [HarmonyPatch(typeof(LevelCoin), "Awake")]
        internal static class Awake {
            // DEBUG
            static bool Prefix(LevelCoin __instance, ref SpriteRenderer ____spriteRenderer, ref bool ____collected) {
		        ____spriteRenderer = __instance.GetComponent<SpriteRenderer>();
                Plugin.Log("Coin: "+__instance.transform.position.x+" : "+__instance.GlobalID);
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelCoin), "Collect")]
        internal static class Collect {
            // DEBUG
            static bool Prefix(ref bool ____collected) {
                Plugin.Log("[Coin] Collected");
                return false;
            }
        }
    }
}
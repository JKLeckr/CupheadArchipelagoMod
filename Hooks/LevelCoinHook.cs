/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class LevelCoinHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Collect));
        }

        [HarmonyPatch(typeof(LevelCoin), "Collect")]
        internal static class Collect {
            static bool Prefix(ref bool ____collected) {
                ____collected = true;
                Plugin.Log("[Coin] Collected");
                return false;
            }
        }
    }
}
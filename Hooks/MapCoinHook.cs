/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class MapCoinHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(MapCoin), "Start")]
        internal static class Start {
            static bool Prefix(string ___coinID, MapCoin __instance) {
                Plugin.Log("Coin "+___coinID);
                return false;
            }
        }
    }
}
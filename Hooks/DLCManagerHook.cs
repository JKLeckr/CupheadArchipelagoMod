/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class DLCManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(DLCEnabled));
        }
    
        [HarmonyPatch(typeof(DLCManager), "DLCEnabled")]
        internal static class DLCEnabled {
            static void Postfix(ref bool __result) {
                if (PlayerData.inGame && APData.IsCurrentSlotEnabled() && !APSettings.UseDLC) {
                    __result = false;
                }
                //Logging.Log($"DLC: {(__result?"on":"off")}");
            }
        }
    }
}

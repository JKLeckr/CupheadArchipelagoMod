/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelLoaderHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            //Harmony.CreateAndPatchAll(typeof(Activate));
        }

        [HarmonyPatch(typeof(MapLevelLoader), "Awake")]
        internal static class Awake {
            static void Postfix(MapLevelLoader __instance) {
                if (__instance is MapLevelLoaderChaliceTutorial) {
                    if (APData.IsCurrentSlotEnabled() && APSettings.DLCChaliceMode == DlcChaliceModes.Disabled) {
                        __instance.gameObject.AddComponent<Disabler>().Init(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapLevelLoader), "Activate")]
        internal static class Activate {}
    }
}

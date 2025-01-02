/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelLoaderHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Activate));
        }

        [HarmonyPatch(typeof(MapLevelLoader), "Activate")]
        internal static class Activate {}
    }
}

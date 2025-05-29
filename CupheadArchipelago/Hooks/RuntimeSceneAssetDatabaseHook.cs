/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.Util;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class RuntimeSceneAssetDatabaseHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(persistentAssets));
            Harmony.CreateAndPatchAll(typeof(persistentAssetsDLC));
        }

        [HarmonyPatch(typeof(RuntimeSceneAssetDatabase), "persistentAssets", MethodType.Getter)]
        internal static class persistentAssets {
            static void Postfix(HashSet<string> __result) {
                Logging.Log("Persistent Assets:");
                Logging.Log($"  {Aux.CollectionToString(__result)}");
            }
        }

        [HarmonyPatch(typeof(RuntimeSceneAssetDatabase), "persistentAssetsDLC", MethodType.Getter)]
        internal static class persistentAssetsDLC {
            static void Postfix(HashSet<string> __result) {
                Logging.Log("Persistent DLC Assets:");
                Logging.Log($"  {Aux.CollectionToString(__result)}");
            }
        }
    }
}

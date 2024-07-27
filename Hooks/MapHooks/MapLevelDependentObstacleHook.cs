/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelDependentObstacleHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            //Harmony.CreateAndPatchAll(typeof(OnConditionNotMet));
            //Harmony.CreateAndPatchAll(typeof(OnConditionMet));
            //Harmony.CreateAndPatchAll(typeof(OnConditionAlreadyMet));
            //Harmony.CreateAndPatchAll(typeof(DoTransition));
            //Harmony.CreateAndPatchAll(typeof(OnChange));
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "Start")]
        internal static class Start {
            static bool Prefix(MapLevelDependentObstacle __instance, GameObject ___ToEnable, GameObject ___ToDisable) {
                if (APData.Initialized&&APData.SData[PlayerData.CurrentSaveFileIndex].enabled&&APSettings.FreemoveIsles) {
                    __instance.OnConditionAlreadyMet();
                    return false;
                }
                else {
                    if (!APData.Initialized) {Plugin.Log("[MapLevelDependentObstacleHook] APData is not Initialized!", LogLevel.Warning);}
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionNotMet")]
        internal static class OnConditionNotMet {
            static void Postfix(MapLevelDependentObstacle __instance) {
                Plugin.Log(__instance+" OnConditionNotMet", LoggingFlags.Debug);
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionMet")]
        internal static class OnConditionMet {
            static void Postfix(MapLevelDependentObstacle __instance) {
                Plugin.Log(__instance+" OnConditionMet", LoggingFlags.Debug);
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionAlreadyMet")]
        internal static class OnConditionAlreadyMet {
            static void Postfix(MapLevelDependentObstacle __instance) {
                Plugin.Log(__instance+" OnConditionAlreadyMet", LoggingFlags.Debug);
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "DoTransition")]
        internal static class DoTransition {
            static void Postfix(MapLevelDependentObstacle __instance) {
                Plugin.Log(__instance+" DoTransition", LoggingFlags.Debug);
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnChange")]
        internal static class OnChange {
            static void Postfix(MapLevelDependentObstacle __instance) {
                Plugin.Log(__instance+" OnChange", LoggingFlags.Debug);
            }
        }
    }
}

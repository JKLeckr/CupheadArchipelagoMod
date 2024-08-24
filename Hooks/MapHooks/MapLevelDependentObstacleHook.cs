/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelDependentObstacleHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnConditionNotMet));
            Harmony.CreateAndPatchAll(typeof(OnConditionMet));
            Harmony.CreateAndPatchAll(typeof(OnConditionAlreadyMet));
            Harmony.CreateAndPatchAll(typeof(DoTransition));
            Harmony.CreateAndPatchAll(typeof(OnChange));
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "Start")]
        internal static class Start {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    __instance.OnConditionAlreadyMet();
                    return false;
                }
                else {
                    if (!APData.Initialized) {Logging.Log("[MapLevelDependentObstacleHook] APData is not Initialized!", LogLevel.Warning);}
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionNotMet")]
        internal static class OnConditionNotMet {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    Logging.Log(__instance+" OnConditionNotMet", LoggingFlags.Debug);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionMet")]
        internal static class OnConditionMet {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    Logging.Log(__instance+" OnConditionMet", LoggingFlags.Debug);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnConditionAlreadyMet")]
        internal static class OnConditionAlreadyMet {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    Logging.Log(__instance+" OnConditionAlreadyMet", LoggingFlags.Debug);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "DoTransition")]
        internal static class DoTransition {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    Logging.Log(__instance+" DoTransition", LoggingFlags.Debug);
                }
                return !(APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles);
            }
        }

        [HarmonyPatch(typeof(MapLevelDependentObstacle), "OnChange")]
        internal static class OnChange {
            static bool Prefix(MapLevelDependentObstacle __instance) {
                if (APData.IsCurrentSlotEnabled()&&APSettings.FreemoveIsles) {
                    Logging.Log(__instance+" OnChange", LoggingFlags.Debug);
                }
                return true;
            }
        }
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapShmupTutorialBridgeActivatorHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(MapShmupTutorialBridgeActivator), "Start")]
        internal static class Start {
            static bool Prefix(int ___dialoguerVariableID, MapLevelDependentObstacle ___blueprintObstacle) {
                if (APData.IsCurrentSlotEnabled()) {
                    Dialoguer.SetGlobalFloat(___dialoguerVariableID, 1f);
                    ___blueprintObstacle.OnConditionAlreadyMet();
                    return false;
                }
                return true;
            }
        }
    }
}
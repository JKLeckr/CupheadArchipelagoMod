/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class KitchenSaltbakerCounterHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        private const int DIALOGUER_ID = 23;

        [HarmonyPatch(typeof(KitchenSaltbakerCounter), "Start")]
        internal static class Start {
            static bool Prefix() {
                Logging.Log($"saltbaker: {Dialoguer.GetGlobalFloat(DIALOGUER_ID)}");
                if (Dialoguer.GetGlobalFloat(23) == 0f && APCondition())
                    Dialoguer.SetGlobalFloat(23, 1f);
                return true;
            }

            private static bool APCondition() {
                if (APData.IsCurrentSlotEnabled()) {
                    return 
                        Config.IsSkippingCutscene(Cutscenes.DLCSaltbakerIntro) ||
                        APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCRequiredIngredients;
                } else return false;
            }
        }
    }
}

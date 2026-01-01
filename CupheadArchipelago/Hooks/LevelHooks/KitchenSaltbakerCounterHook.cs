/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Config;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class KitchenSaltbakerCounterHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        private const int DIALOGUER_ID = KitchenLevelHook.DIALOGUER_ID;

        [HarmonyPatch(typeof(KitchenSaltbakerCounter), "Start")]
        internal static class Start {
            static bool Prefix() {
                Logging.Log($"saltbaker: {Dialoguer.GetGlobalFloat(DIALOGUER_ID)}");
                if (APData.IsCurrentSlotEnabled()) {
                    if (Dialoguer.GetGlobalFloat(DIALOGUER_ID) == 0f) {
                        KitchenLevelHook.firstVisit = true;
                        if (APCondition()) Dialoguer.SetGlobalFloat(DIALOGUER_ID, 1f);
                    } else KitchenLevelHook.firstVisit = false;
                }
                return true;
            }

            private static bool APCondition() {
                return 
                    MConf.IsSkippingCutscene(Cutscenes.DLCSaltbakerIntro) ||
                    APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCRequiredIngredients;
            }
        }
    }
}

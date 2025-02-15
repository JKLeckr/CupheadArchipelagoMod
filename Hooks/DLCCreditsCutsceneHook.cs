/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class DLCCreditsCutsceneHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(goToNext));
        }

        [HarmonyPatch(typeof(DLCCreditsCutscene), "goToNext")]
        internal static class goToNext {
            static bool Prefix() {
                if (APData.IsCurrentSlotEnabled()) {
                    SceneLoader.LoadLastMap();
                    return false;
                }
                return true;
            }
        }
    }
}

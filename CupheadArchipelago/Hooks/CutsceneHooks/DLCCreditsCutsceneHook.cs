/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.CutsceneHooks {
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

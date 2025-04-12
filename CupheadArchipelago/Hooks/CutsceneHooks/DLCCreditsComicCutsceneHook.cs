/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.CutsceneHooks {
    internal class DLCCreditsComicCutsceneHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(CreditsScreen), "Start")]
        internal static class Start {
            static bool Prefix() {
                APClient.SendChecks(true);
                return true;
            }
        }
    }
}

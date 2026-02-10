/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.CutsceneHooks {
    internal class DLCCreditsComicCutsceneHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(DLCCreditsComicCutscene), "Start")]
        internal static class Start {
            static bool Prefix(DLCCreditsComicCutscene __instance) {
                __instance.StartCoroutine(SendChecks_cr());
                return true;
            }

            private static IEnumerator SendChecks_cr() {
                while (SceneLoader.CurrentlyLoading) {
                    yield return null;
                }
                APClient.SendChecks(true);
            }
        }
    }
}

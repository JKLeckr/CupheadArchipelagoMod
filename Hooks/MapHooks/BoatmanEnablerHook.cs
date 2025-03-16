/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Reflection;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class BoatmanEnablerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(BoatmanEnabler), "Start")]
        internal static class Start {
            private static MethodInfo _mi_check_cr = typeof(BoatmanEnabler).GetMethod("check_cr", BindingFlags.NonPublic | BindingFlags.Instance);

            static bool Prefix(BoatmanEnabler __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (DLCManager.DLCEnabled()) __instance.StartCoroutine(apcheck_cr(__instance));
                    return false;
                }
                else return true;
            }
            private static IEnumerator apcheck_cr(BoatmanEnabler instance) {
                while (!APClient.APSessionGSPlayerData.dlc_boat || !IsInReadyState()) {
                    yield return null;
                }
                instance.StartCoroutine(_mi_check_cr.Name);
                yield break;
            }
            private static bool IsInReadyState() {
                return Map.Current.CurrentState == Map.State.Ready && AbstractPauseGUIHook.CanPause && !AbstractPauseGUIHook.Paused;
            }
        }
    }
}

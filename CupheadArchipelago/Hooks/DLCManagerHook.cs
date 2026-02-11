/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class DLCManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(DLCEnabled));
            Harmony.CreateAndPatchAll(typeof(CheckInstallationStatusChanged));
        }

        private static bool disableDLC = false;

        internal static void Reset() {
            disableDLC = false;
        }
        internal static void DisableDLC() {
            disableDLC = true;
        }
        internal static bool DLCDisabled() => disableDLC;

        [HarmonyPatch(typeof(DLCManager), "DLCEnabled")]
        internal static class DLCEnabled {
            static void Postfix(ref bool __result) {
                if (disableDLC) {
                    __result = false;
                }
                //Logging.Log($"DLC: {(__result?"on":"off")}");
            }
        }

        [HarmonyPatch(typeof(DLCManager), "CheckInstallationStatusChanged")]
        internal static class CheckInstallationStatusChanged {
            static bool Prefix() {
                return !disableDLC;
            }
        }
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class CupheadHook {
        public static void Hook() {}

        // TODO: Add a control panel attached to this.

        [HarmonyPatch(typeof(Cuphead), "Update")]
        internal static class Update {
            static void Postfix() {}
        }
    }
}
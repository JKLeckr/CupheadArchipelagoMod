/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class OnlineInterfaceHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(UnlockAchievement));
        }

        [HarmonyPatch(typeof(OnlineInterface), "UnlockAchievement")]
        internal static class UnlockAchievement {
            static bool Prefix() {
                Plugin.Log("UnlockAchievement");
                if (APData.IsCurrentSlotEnabled()) return false;
                return true;
            }
        }
    }
}
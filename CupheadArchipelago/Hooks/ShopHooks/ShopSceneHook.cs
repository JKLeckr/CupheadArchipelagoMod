/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopSceneHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnExit));
        }

        [HarmonyPatch(typeof(ShopScene), "OnExit")]
        internal static class OnExit {
            static void Postfix() {
                if (APData.IsCurrentSlotEnabled() && ShopHookBase.APIsAllItemsBought() && !APClient.AreGoalsCompleted(Goals.ShopBuyout)) {
                    APClient.GoalComplete(Goals.ShopBuyout, true);
                }
            }
        }
    }
}

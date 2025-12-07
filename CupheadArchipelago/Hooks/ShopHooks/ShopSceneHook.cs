/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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

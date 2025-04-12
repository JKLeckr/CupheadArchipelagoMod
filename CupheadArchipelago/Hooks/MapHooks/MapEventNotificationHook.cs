/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapEventNotificationHook {
        // TODO: Add notifications for sending and maybe receiving checks too!
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(ShowTooltipEvent));
        }

        [HarmonyPatch(typeof(MapEventNotification), "ShowTooltipEvent")]
        internal static class ShowTooltipEvent {
            static bool Prefix(TooltipEvent tooltipEvent) {
                if (APData.IsCurrentSlotEnabled() && tooltipEvent == TooltipEvent.BackToKitchen)
                    return false;
                else return true;
            }
        }
    }
}

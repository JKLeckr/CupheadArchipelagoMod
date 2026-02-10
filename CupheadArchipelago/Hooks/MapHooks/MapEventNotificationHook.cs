/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

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

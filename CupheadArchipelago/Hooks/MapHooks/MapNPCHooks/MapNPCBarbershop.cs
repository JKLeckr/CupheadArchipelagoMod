/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCBarbershopHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        private static readonly long locationId = APLocation.quest_4mel;

        [HarmonyPatch(typeof(MapNPCBarbershop), "Start")]
        internal static class Start {
            static bool Prefix(int ___dialoguerVariableID) {
                LogDialoguerGlobalFloat(___dialoguerVariableID);
                if (APData.IsCurrentSlotEnabled()) {
                    if (Dialoguer.GetGlobalFloat(___dialoguerVariableID) == 0 && APClient.IsLocationChecked(locationId)) {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 1f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                    }
                }
                return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) =>
            Logging.Log($"{nameof(MapNPCBarbershop)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}

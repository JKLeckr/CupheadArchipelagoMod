/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapNPCBarbershopHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        [HarmonyPatch(typeof(MapNPCBarbershop), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {}
    }
}

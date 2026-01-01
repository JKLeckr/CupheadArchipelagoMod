/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCChaliceFanBHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnDialogueEnded));
        }

        private static readonly long locationId = APLocation.dlc_quest_cactusgirl;
            

        [HarmonyPatch(typeof(MapNPCChaliceFanB), "OnDialogueEnded")]
        internal static class OnDialogueEnded {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                return MapNPCHookBase.
                        MapNPCQuestHookBase.
                        MapNPCQuestHookTranspiler(instructions, locationId);
            }
        }
    }
}

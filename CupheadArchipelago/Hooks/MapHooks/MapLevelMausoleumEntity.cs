/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelMausoleumEntityHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(ValidateSucess));
            Harmony.CreateAndPatchAll(typeof(ValidateCondition));
        }

        [HarmonyPatch(typeof(MapLevelMausoleumEntity), "ValidateSucess")]
        internal static class ValidateSucess {
            static void Postfix(ref bool __result) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"[MapLevelMausoleumEntity] ValidateSuccess: {__result}");
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(MapLevelMausoleumEntity), "ValidateCondition")]
        internal static class ValidateCondition {
            static void Postfix(ref bool __result) {
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"[MapLevelMausoleumEntity] ValidateCondition: {__result}");
                    __result = false;
                }
            }
        }
    }
}

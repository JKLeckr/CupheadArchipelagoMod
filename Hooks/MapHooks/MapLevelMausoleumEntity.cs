/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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
                Logging.Log($"[MapLevelMausoleumEntity] ValidateSuccess: {__result}");
                __result = false;
            }
        }

        [HarmonyPatch(typeof(MapLevelMausoleumEntity), "ValidateCondition")]
        internal static class ValidateCondition {
            static void Postfix(ref bool __result) {
                Logging.Log($"[MapLevelMausoleumEntity] ValidateCondition: {__result}");
                __result = false;
            }
        }
    }
}
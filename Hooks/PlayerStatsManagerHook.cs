/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class PlayerStatsManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(DebugFillSuper));
        }

        public const float DEFAULT_SUPER_ADD_AMOUNT = 10f;
        public const float DEFAULT_SUPER_FILL_AMOUNT = 50f;
        public const bool DEFAULT_PLAY_SUPER_CHANGED_EFFECT = true;

        private static float superFillAmount = DEFAULT_SUPER_FILL_AMOUNT;
        private static bool playSuperChangedEffect = DEFAULT_PLAY_SUPER_CHANGED_EFFECT;

        private static MethodInfo _mi_set_SuperMeter = typeof(PlayerStatsManager).GetProperty("SuperMeter").GetSetMethod(true);
        private static MethodInfo _mi_OnSuperChanged = typeof(PlayerStatsManager).GetMethod("OnSuperChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(PlayerStatsManager), "DebugFillSuper")]
        internal static class DebugFillSuper {
            static bool Prefix(PlayerStatsManager __instance) {
                _mi_set_SuperMeter.Invoke(__instance, new object[]{superFillAmount});
                _mi_OnSuperChanged.Invoke(__instance, new object[]{playSuperChangedEffect});
                return false;
            }
        }

        public static float GetSuperFillAmount() => superFillAmount;
        public static void SetSuperFillAmount(float set) => superFillAmount = set;

        public static void AddEx(PlayerStatsManager instance, float add) {
            SetSuper(instance, instance.SuperMeter + add);
        }
        public static void SetSuper(PlayerStatsManager instance, float set) {
            float orig = superFillAmount;
            superFillAmount = set;
            instance.DebugFillSuper();
            superFillAmount = orig;
        }
    }
}
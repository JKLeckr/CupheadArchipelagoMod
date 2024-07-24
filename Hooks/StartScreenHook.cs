/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class StartScreenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(StartScreen), "Awake")]
        internal static class Awake {
            static void Postfix() {
                APClient.CloseArchipelagoSession();
            }
        }
    }
}
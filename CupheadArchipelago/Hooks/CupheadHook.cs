/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Unity;
using UnityEngine;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class CupheadHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(Cuphead), "Awake")]
        internal static class Awake {
            static void Postfix() {
                new GameObject("ControlBoard", typeof(ControlBoard)).SetActive(false);
            }
        }
    }
}

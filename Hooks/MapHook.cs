/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class MapHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            //Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(Map), "Awake")]
        internal static class Awake {
            static void Postfix(Map __instance, CupheadMapCamera ___cupheadMapCamera) {
                Plugin.Log(Level.Difficulty, LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks();
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    apmngr.Init(APManager.Type.Normal);
                }
            }
        }
    }
}
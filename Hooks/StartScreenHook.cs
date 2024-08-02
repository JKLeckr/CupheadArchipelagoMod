/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using Archipelago.MultiClient.Net.Packets;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class StartScreenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(StartScreen), "Awake")]
        internal static class Awake {
            static bool Prefix() {
                return Plugin.State>=0;
            }
            static void Postfix() {
                APClient.CloseArchipelagoSession();
            }
        }

        [HarmonyPatch(typeof(StartScreen), "Start")]
        internal static class Start {
            static bool Prefix() {
                if (Plugin.State<0) {
                    Plugin.LogFatal("Errors occured. Aborting to prevent damage!");
                    return false;
                }
                return true;
            }
        }
    }
}
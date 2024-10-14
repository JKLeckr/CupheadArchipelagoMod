/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(Map), "Awake")]
        internal static class Awake {
            static void Postfix(Map __instance, CupheadMapCamera ___cupheadMapCamera, Scenes ___scene) {
                Logging.Log(Level.Difficulty, LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks();
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    switch (___scene) {
                        case Scenes.scene_map_world_1:
                            APClient.APSessionGSData.map |= 1;
                            break;
                        case Scenes.scene_map_world_2:
                            APClient.APSessionGSData.map |= 2;
                            break;
                        case Scenes.scene_map_world_3:
                            APClient.APSessionGSData.map |= 4;
                            break;
                        case Scenes.scene_map_world_4:
                            APClient.APSessionGSData.map |= 8;
                            break;
                        case Scenes.scene_map_world_DLC:
                            APClient.APSessionGSData.map |= 16;
                            break;
                    }
                    apmngr.Init(APManager.Type.Normal);
                    apmngr.SetActive(true);
                }
            }
        }
    }
}
/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Text;
using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        internal static readonly Scenes[] mapScenes = [
            Scenes.scene_map_world_1,
            Scenes.scene_map_world_2,
            Scenes.scene_map_world_3,
            Scenes.scene_map_world_4,
            Scenes.scene_map_world_DLC,
        ];

        [HarmonyPatch(typeof(Map), "Awake")]
        internal static class Awake {
            static void Postfix(Map __instance, Scenes ___scene, CupheadMapCamera ___cupheadMapCamera) {
                Logging.Log(Level.Difficulty, LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks();
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    Logging.Log($"Current Map Scene: {___scene}");
                    LogMapsVisited();
                    apmngr.Init(APManager.Type.Normal);
                    apmngr.SetActive(true);
                }
            }

            private static bool MapSessionStarted(Scenes mapScene) {
                PlayerData.MapData mapData = PlayerData.Data.GetMapData(mapScene);
                return mapData.sessionStarted;
            }
            private static void LogMapsVisited() {
                StringBuilder res = new();
                for (int i=0;i<mapScenes.Length;i++) {
                    res.Append($"{mapScenes[i]}");
                    if (i<mapScenes.Length-1) res.Append(",");
                }
                Logging.Log($"Visited worlds: {res}");
            }
        }
    }
}
/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(start_cr));
        }

        internal static readonly Scenes[] mapScenes = [
            Scenes.scene_map_world_1,
            Scenes.scene_map_world_2,
            Scenes.scene_map_world_3,
            Scenes.scene_map_world_4,
            Scenes.scene_map_world_DLC
        ];
        internal static readonly string[] mapNames = [
            "w1",
            "w2",
            "w3",
            "wh",
            "w4"
        ];

        [HarmonyPatch(typeof(Map), "Awake")]
        internal static class Awake {
            static void Postfix(Map __instance, Scenes ___scene, CupheadMapCamera ___cupheadMapCamera) {
                Logging.Log(Level.Difficulty, LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    Logging.Log($"Current Map Scene: {___scene}");
                    try {
                        APClient.APSessionDataStorage[Scope.Slot, "current_map"].Initialize(0);
                        APClient.APSessionDataStorage[Scope.Slot, "current_map"] = ___scene switch {
                            Scenes.scene_map_world_1 => 0,
                            Scenes.scene_map_world_2 => 1,
                            Scenes.scene_map_world_3 => 2,
                            Scenes.scene_map_world_4 => 3,
                            Scenes.scene_map_world_DLC => 4,
                            _ => -1
                        };
                        Logging.Log($"Successfully wrote 'current_map' to DataStorage.");
                    }
                    catch (Exception e) {
                        Logging.LogWarning($"Failed to write 'current_map' to DataStorage: {e.Message}");
                    }
                    RecordMapsVisited();
                    apmngr.Init(APManager.MngrType.Normal);
                    apmngr.SetActive(true);
                    __instance.StartCoroutine(UpdateCoins_cr());
                    __instance.StartCoroutine(SendChecks_cr());
                }
            }

            private static IEnumerator SendChecks_cr() {
                while (SceneLoader.CurrentlyLoading) {
                    yield return null;
                }
                APClient.UpdateGoalFlags();
                APClient.SendChecks(true);
            }

            private static bool MapSessionStarted(Scenes mapScene) {
                PlayerData.MapData mapData = PlayerData.Data.GetMapData(mapScene);
                return mapData?.sessionStarted ?? false;
            }
            private static void RecordMapsVisited() {
                StringBuilder res = new();
                int mapbits = 0;
                for (int i = 0; i < mapScenes.Length; i++) {
                    if (MapSessionStarted(mapScenes[i])) {
                        mapbits |= 1 << i;
                        res.Append($"{mapNames[i]} ");
                    }
                }
                try {
                    APClient.APSessionDataStorage[Scope.Slot, "maps_visited"].Initialize(0);
                    APClient.APSessionDataStorage[Scope.Slot, "maps_visited"] += Bitwise.Or(mapbits);
                    Logging.Log($"Successfully wrote 'maps_visited' to DataStorage.");
                } catch (Exception e) {
                    Logging.LogWarning($"Failed to write 'maps_visited' to DataStorage: {e.Message}");
                }
                Logging.Log($"Visited worlds: {res}");
            }
            private static IEnumerator UpdateCoins_cr() {
                Logging.Log("Updating Coins...");
                yield return null;
                HashSet<Levels> platformLevels = new(Level.platformingLevels);
                List<PlayerData.PlayerCoinManager.LevelAndCoins> levelsAndCoins = PlayerData.Data.coinManager.LevelsAndCoins;
	            foreach (PlayerData.PlayerCoinManager.LevelAndCoins levelAndCoins in levelsAndCoins) {
                    //Logging.Log($"Coin Level: {levelAndCoins.level}");
                    if (LevelLocationMap.LevelHasLocations(levelAndCoins.level) && platformLevels.Contains(levelAndCoins.level)) {
                        if (!levelAndCoins.Coin1Collected) {
                            long loc = LevelLocationMap.GetLocationId(levelAndCoins.level, 4);
                            levelAndCoins.Coin1Collected = levelAndCoins.Coin1Collected || APClient.IsLocationChecked(loc);
                        }
			            if (!levelAndCoins.Coin2Collected) {
                            long loc = LevelLocationMap.GetLocationId(levelAndCoins.level, 5);
                            levelAndCoins.Coin2Collected = levelAndCoins.Coin2Collected || APClient.IsLocationChecked(loc);
                        }
			            if (!levelAndCoins.Coin3Collected) {
                            long loc = LevelLocationMap.GetLocationId(levelAndCoins.level, 6);
                            levelAndCoins.Coin3Collected = levelAndCoins.Coin3Collected || APClient.IsLocationChecked(loc);
                        }
			            if (!levelAndCoins.Coin4Collected) {
                            long loc = LevelLocationMap.GetLocationId(levelAndCoins.level, 7);
                            levelAndCoins.Coin4Collected = levelAndCoins.Coin4Collected || APClient.IsLocationChecked(loc);
                        }
			            if (!levelAndCoins.Coin5Collected) {
                            long loc = LevelLocationMap.GetLocationId(levelAndCoins.level, 8);
                            levelAndCoins.Coin5Collected = levelAndCoins.Coin5Collected || APClient.IsLocationChecked(loc);
                        }
                    }
                    yield return null;
	            }
            }
        }

        [HarmonyPatch(typeof(Map), "start_cr", MethodType.Enumerator)]
        internal static class start_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                int se_success = 0;
                int set_success = 0;
                bool debug = false;

                MethodInfo _mi_MapEventNotification_get_Current = typeof(MapEventNotification).GetProperty("Current", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_ShowEvent = typeof(MapEventNotification).GetMethod("ShowEvent", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_ShowTooltipEvent = typeof(MapEventNotification).GetMethod("ShowTooltipEvent", BindingFlags.Public | BindingFlags.Instance);

                MethodInfo _mi_APShowEvent = typeof(start_cr).GetMethod("APShowEvent", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APShowTooltipEvent = typeof(start_cr).GetMethod("APShowTooltipEvent", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 2; i++) {
                    if (
                        codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_MapEventNotification_get_Current &&
                        codes[i + 2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 2].operand == _mi_ShowEvent
                    ) {
                        codes[i + 2].opcode = OpCodes.Call;
                        codes[i + 2].operand = _mi_APShowEvent;
                        se_success++;
                    }
                    if (
                        codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_MapEventNotification_get_Current &&
                        codes[i + 2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 2].operand == _mi_ShowTooltipEvent
                    ) {
                        codes[i + 2].opcode = OpCodes.Call;
                        codes[i + 2].operand = _mi_APShowTooltipEvent;
                        set_success++;
                    }
                }
                if (se_success != 8 && set_success != 4)
                    throw new Exception($"{nameof(start_cr)}: Patch Failed! {se_success}, {set_success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void APShowEvent(MapEventNotification current, MapEventNotification.Type eventType) {
                if (APData.IsCurrentSlotEnabled()) {
                    switch (eventType) {
                        case MapEventNotification.Type.AirplaneIngredient:
                        case MapEventNotification.Type.RumIngredient:
                        case MapEventNotification.Type.OldManIngredient:
                        case MapEventNotification.Type.SnowIngredient:
                        case MapEventNotification.Type.CowboyIngredient:
                        case MapEventNotification.Type.SoulContract: break;
                        default: current.ShowEvent(eventType); break;
                    }
                }
                else current.ShowEvent(eventType);
            }
            private static void APShowTooltipEvent(MapEventNotification current, TooltipEvent tooltipEvent) {
                if (APData.IsCurrentSlotEnabled()) {
                    switch (tooltipEvent) {
                        case TooltipEvent.BackToKitchen: break;
                        default: current.ShowTooltipEvent(tooltipEvent); break;
                    }
                } else {
                    current.ShowTooltipEvent(tooltipEvent);
                }
            }
        }
    }
}

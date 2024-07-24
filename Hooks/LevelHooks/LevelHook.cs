/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class LevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(_OnLevelStart));
            Harmony.CreateAndPatchAll(typeof(_OnLevelEnd));
            Harmony.CreateAndPatchAll(typeof(_OnPreWin));
            Harmony.CreateAndPatchAll(typeof(zHack_OnWin));
        }

        [HarmonyPatch(typeof(Level), "Awake")]
        internal static class Awake {
            /*static bool Prefix() {
                Level.SetCurrentMode(GetClamppedCurrentLevelMode());
                return true;
            }*/
            static void Postfix(Level __instance) {
                Plugin.Log($"LIndex: {__instance.mode}", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks();
                }
            }

            /*private static Level.Mode GetClamppedCurrentLevelMode() {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    //Plugin.Log.LogInfo("Clamping mode");
                    if (APData.CurrentSData.Hard||Level.CurrentMode>0) {
                        return APData.CurrentSData.Hard?Level.Mode.Hard:Level.Mode.Normal;
                    } else return Level.Mode.Easy;
                }
                else return Level.CurrentMode;
            }*/
        }

        [HarmonyPatch(typeof(Level), "_OnLevelStart")]
        internal static class _OnLevelStart {
            static void Postfix(Level __instance) {
                Plugin.Log("_OnLevelStart", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    apmngr.Init(APManager.Type.Level);
                }
            }
        }

        [HarmonyPatch(typeof(Level), "_OnLevelEnd")]
        internal static class _OnLevelEnd {
            static bool Prefix(Level __instance) {
                Plugin.Log("_OnLevelEnd", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()&&APManager.Current!=null)
                    APManager.Current.SetActive(false);
                return true;
            }
        }

        [HarmonyPatch(typeof(Level), "_OnPreWin")]
        internal static class _OnPreWin {
            static void Postfix(Level __instance) {
                Plugin.Log("_OnPreWin", LoggingFlags.Debug);
                APCheck(__instance);
            }

            private static void APCheck(Level instance) {
                Plugin.Log("[LevelHook] APCheck");
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log($"[LevelHook] Level: {instance.CurrentLevel}");
                    // For now, the final bosses are not checks because they are event locations
                    if (instance.CurrentLevel == Levels.Devil || instance.CurrentLevel == Levels.Saltbaker) {
                        Plugin.Log("[LevelHook] Goal");
                        APClient.GoalComplete((instance.CurrentLevel == Levels.Saltbaker)?Goal.Saltbaker:Goal.Devil);
                    }
                    else if (instance.CurrentLevel == Levels.Mausoleum) {
                        Plugin.Log("[LevelHook] Mausoleum Type");
                        switch (PlayerData.Data.CurrentMap)
		                {
		                    case Scenes.scene_map_world_1:
			                    APClient.Check(APLocation.level_mausoleum_i);
			                    break;
		                    case Scenes.scene_map_world_2:
			                    APClient.Check(APLocation.level_mausoleum_ii);
			                    break;
		                    case Scenes.scene_map_world_3:
			                    APClient.Check(APLocation.level_mausoleum_iii);
			                    break;
                            default:
                                Plugin.LogWarning($"[LevelHook] Invalid Mausoleum Map: {PlayerData.Data.CurrentMap}");
                                break;
		                }
                    }
                    else if (!Level.IsInBossesHub) {
                        Level.Mode normalMode = APSettings.Hard?Level.Mode.Hard:Level.Mode.Normal;
                        if (Level.Difficulty >= normalMode) {
                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,0), false);
                            switch (instance.LevelType) {
                                case Level.Type.Battle: {
                                    Plugin.Log("[LevelHook] Battle Type");
                                    if (APSettings.BossGradeChecks>0)
                                        if (Level.Grade>=(LevelScoringData.Grade.AMinus+((int)APSettings.BossGradeChecks))) {
                                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,1), false);
                                        }
                                    break;
                                }
                                case Level.Type.Platforming: {
                                    Plugin.Log("[LevelHook] Platforming Type");
                                    if (APSettings.RungunGradeChecks>0) {
                                        if (Level.Grade>=(LevelScoringData.Grade.AMinus+((int)APSettings.RungunGradeChecks))) {
                                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,((int)APSettings.RungunGradeChecks>3)?2:1), false);
                                        }
                                    }
                                    break;
                                }
                                default: {
                                    Plugin.Log("[LevelHook] Other Level Type");
                                    break;
                                }
                            }
                        } else {
                            Plugin.Log("[LevelHook] Difficulty needs to be higher for there to be checks");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Level), "zHack_OnWin")]
        internal static class zHack_OnWin {
            static bool Prefix(Level __instance) {
                Plugin.Log("zHack_OnWin", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()&&APManager.Current!=null)
                    APManager.Current.SetActive(false);
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                MethodInfo _mi_get_mode = typeof(Level).GetProperty("mode").GetGetMethod();
                MethodInfo _mi_set_Difficulty = typeof(Level).GetProperty("Difficulty").GetSetMethod(true);
                MethodInfo _mi_HackDifficulty = typeof(zHack_OnWin).GetMethod("HackDifficulty", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i = 0; i < codes.Count - 2; i++) {
                    if (codes[i].opcode==OpCodes.Call && (MethodInfo)codes[i].operand==_mi_get_mode &&
                        codes[i+1].opcode==OpCodes.Call && (MethodInfo)codes[i+1].operand==_mi_set_Difficulty) {
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_HackDifficulty));
                        if (debug) Plugin.Log("Patch success");
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(zHack_OnWin)}: Patch Failed!");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static Level.Mode HackDifficulty(Level.Mode mode) {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    return (APSettings.Hard&&mode<Level.Mode.Hard)?Level.Mode.Easy:mode;
                }
                else return mode;
            }
        }
    }
}
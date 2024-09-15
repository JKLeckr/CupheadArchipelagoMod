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
            Harmony.CreateAndPatchAll(typeof(_OnLose));
            Harmony.CreateAndPatchAll(typeof(_OnPreWin));
            Harmony.CreateAndPatchAll(typeof(zHack_OnWin));
        }

        private static readonly Dictionary<Levels, string> bossNames = new() {
            {Levels.Veggies, "The Root Pack"},
            {Levels.Slime, "Goopy Le Grande"},
            {Levels.FlyingBlimp, "Hilda Berg"},
            {Levels.Flower, "Cagney Carnation"},
            {Levels.Frogs, "Ribby and Croaks"},
            {Levels.Baroness, "Barnoness Von Bon Bon"},
            {Levels.Clown, "Beppi the Clown"},
            {Levels.FlyingGenie, "Djimmi the Great"},
            {Levels.Dragon, "Grim Matchstick"},
            {Levels.FlyingBird, "Wally Warbles"},
            {Levels.Bee, "Rumor Honeybottoms"},
            {Levels.Pirate, "Captain Brineybeard"},
            {Levels.SallyStagePlay, "Sally Stageplay"},
            {Levels.Mouse, "Werner Werman"},
            {Levels.Robot, "Dr. Kahl's Robot"},
            {Levels.FlyingMermaid, "Cala Maria"},
            {Levels.Train, "The Phanton Express"},
            {Levels.DicePalaceBooze, "The Tipsy Troop"},
            {Levels.DicePalaceChips, "Chips Bettigan"},
            {Levels.DicePalaceCigar, "Mr. Wheezy"},
            {Levels.DicePalaceDomino, "Pip and Dot"},
            {Levels.DicePalaceRabbit, "Hopus Pocus"},
            {Levels.DicePalaceFlyingHorse, "Phear Lap"},
            {Levels.DicePalaceRoulette, "Pirouletta"},
            {Levels.DicePalaceEightBall, "Mangosteen"},
            {Levels.DicePalaceFlyingMemory, "Mr. Chimes"},
            {Levels.DicePalaceMain, "King Dice"},
            {Levels.Devil, "The Devil"},
            {Levels.OldMan, "Glumstone the Giant"},
            {Levels.SnowCult, "Mortimer Freeze"},
            {Levels.RumRunners, "The Moonshine Mob"},
            {Levels.FlyingCowboy, "Esther Winchester"},
            {Levels.Airplane, "The Howling Aces"},
            {Levels.Saltbaker, "Chef Saltbaker"},
            {Levels.Graveyard, "The Angel and The Demon"},
            {Levels.ChessPawn, "The Pawns"},
            {Levels.ChessKnight, "The Knight"},
            {Levels.ChessBishop, "The Bishop"},
            {Levels.ChessRook, "The Rook"},
            {Levels.ChessQueen, "The Queen"},
        };
        private static readonly Dictionary<Levels, string> platformLevelNames = new() {
            {Levels.Platforming_Level_1_1, "Forest Follies"},
            {Levels.Platforming_Level_1_2, "Treetop Trouble"},
            {Levels.Platforming_Level_2_1, "Funhouse Frazzle"},
            {Levels.Platforming_Level_2_2, "Funfair Fever"},
            {Levels.Platforming_Level_3_1, "Perilous Piers"},
            {Levels.Platforming_Level_3_2, "Rugged Ridge"},
        };

        [HarmonyPatch(typeof(Level), "Awake")]
        internal static class Awake {
            /*static bool Prefix() {
                Level.SetCurrentMode(GetClamppedCurrentLevelMode());
                return true;
            }*/
            static void Postfix(Level __instance) {
                Logging.Log($"LIndex: {__instance.mode}", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks();
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    apmngr.Init(IsValidLevel(__instance) ? APManager.Type.Level : APManager.Type.Normal);
                }
            }

            /*private static Level.Mode GetClamppedCurrentLevelMode() {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    //Logging.Log.LogInfo("Clamping mode");
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
                Logging.Log("_OnLevelStart", LoggingFlags.Debug);
                APManager.Current?.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(Level), "_OnLevelEnd")]
        internal static class _OnLevelEnd {
            static bool Prefix(Level __instance) {
                Logging.Log("_OnLevelEnd", LoggingFlags.Debug);
                APManager.Current?.SetActive(false);
                return true;
            }
        }

        [HarmonyPatch(typeof(Level), "_OnPreWin")]
        internal static class _OnPreWin {
            static void Postfix(Level __instance) {
                Logging.Log("_OnPreWin", LoggingFlags.Debug);
                APCheck(__instance);
            }

            private static void APCheck(Level instance) {
                Logging.Log("[LevelHook] APCheck");
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"[LevelHook] Level: {instance.CurrentLevel}");
                    // For now, the final bosses are not checks because they are event locations
                    if (instance.CurrentLevel == Levels.Devil || instance.CurrentLevel == Levels.Saltbaker) {
                        Logging.Log("[LevelHook] Goal");
                        APClient.GoalComplete((instance.CurrentLevel == Levels.Saltbaker)?Goals.Saltbaker:Goals.Devil);
                    }
                    else if (instance.CurrentLevel == Levels.Mausoleum) {
                        Logging.Log("[LevelHook] Mausoleum Type");
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
                                Logging.LogWarning($"[LevelHook] Invalid Mausoleum Map: {PlayerData.Data.CurrentMap}");
                                break;
		                }
                    }
                    else if (!Level.IsInBossesHub || instance.CurrentLevel == Levels.DicePalaceMain) {
                        Level.Mode normalMode = APSettings.Hard?Level.Mode.Hard:Level.Mode.Normal;
                        if (Level.Difficulty >= normalMode) {
                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,0), false);
                            switch (instance.LevelType) {
                                case Level.Type.Battle: {
                                    Logging.Log("[LevelHook] Battle Type");
                                    if (APSettings.BossGradeChecks>0)
                                        if (Level.Grade>=(LevelScoringData.Grade.AMinus+(((int)APSettings.BossGradeChecks)-1))) {
                                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,1), false);
                                        }
                                    break;
                                }
                                case Level.Type.Platforming: {
                                    Logging.Log("[LevelHook] Platforming Type");
                                    if (APSettings.RungunGradeChecks>0) {
                                        if (Level.Grade>=(LevelScoringData.Grade.AMinus+(((int)APSettings.RungunGradeChecks)-1))) {
                                            APClient.Check(LevelLocationMap.GetLocationId(instance.CurrentLevel,((int)APSettings.RungunGradeChecks>3)?2:1), false);
                                        }
                                    }
                                    break;
                                }
                                default: {
                                    Logging.Log("[LevelHook] Other Level Type");
                                    break;
                                }
                            }
                        } else {
                            Logging.Log("[LevelHook] Difficulty needs to be higher for there to be checks");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Level), "_OnLose")]
        internal static class _OnLose {
            static bool Prefix(Level __instance) {
                if (APData.IsCurrentSlotEnabled() && APClient.IsDeathLinkActive()) {
                    Logging.Log("DeathLink");
                    if (APManager.Current != null && !APManager.Current.IsDeathTriggered()) {
                        if (__instance.LevelType == Level.Type.Platforming) {
                            APClient.SendDeathLink(platformLevelNames[__instance.CurrentLevel], DeathLinkCauseType.Normal);
                        }
                        else if (__instance.LevelType == Level.Type.Tutorial) {
                            APClient.SendDeathLink(__instance.CurrentLevel.ToString(), DeathLinkCauseType.Tutorial);
                        }
                        else {
                            if (__instance.CurrentLevel == Levels.Mausoleum) {
                                APClient.SendDeathLink("Mausoleum", DeathLinkCauseType.Mausoleum);
                            }
                            else if (Array.Exists(Level.kingOfGamesLevels, (Levels level) => __instance.CurrentLevel == level)) {
                                APClient.SendDeathLink(bossNames[__instance.CurrentLevel], DeathLinkCauseType.ChessCastle);
                            }
                            else if (__instance.CurrentLevel == Levels.Graveyard) {
                                APClient.SendDeathLink(bossNames[Levels.Graveyard], DeathLinkCauseType.Graveyard);
                            }
                            else {
                                APClient.SendDeathLink(bossNames[__instance.CurrentLevel], DeathLinkCauseType.Boss);
                            }
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Level), "zHack_OnWin")]
        internal static class zHack_OnWin {
            static bool Prefix(Level __instance) {
                Logging.Log("zHack_OnWin", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()&&APManager.Current!=null)
                    APManager.Current.SetActive(false);
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                MethodInfo _mi_get_mode = typeof(Level).GetProperty("mode")?.GetGetMethod();
                MethodInfo _mi_set_Difficulty = typeof(Level).GetProperty("Difficulty")?.GetSetMethod(true);
                MethodInfo _mi_HackDifficulty = typeof(zHack_OnWin).GetMethod("HackDifficulty", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i = 0; i < codes.Count - 2; i++) {
                    if (codes[i].opcode==OpCodes.Call && (MethodInfo)codes[i].operand==_mi_get_mode &&
                        codes[i+1].opcode==OpCodes.Call && (MethodInfo)codes[i+1].operand==_mi_set_Difficulty) {
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_HackDifficulty));
                        if (debug) Logging.Log("Patch success");
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(zHack_OnWin)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
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

        private static bool IsValidLevel(Level instance) {
            Logging.Log($"Level: {instance.CurrentLevel} LevelType: {instance.LevelType}");
            return instance.LevelType switch {
                Level.Type.Battle or Level.Type.Platforming or Level.Type.Tutorial => true,
                _ => false,
            } && instance.CurrentLevel switch {
                Levels.House or Levels.ShmupTutorial or Levels.DiceGate or Levels.ChessCastle => false,
                _ => true,
            };
        }
    }
}
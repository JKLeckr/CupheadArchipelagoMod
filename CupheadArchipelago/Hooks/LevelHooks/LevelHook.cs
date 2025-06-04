/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Unity;
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
            static void Postfix(Level __instance) {
                Logging.Log($"LIndex: {__instance.mode}", LoggingFlags.Debug);
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.SendChecks(true);
                    APManager apmngr = __instance.gameObject.AddComponent<APManager>();
                    apmngr.Init(GetLevelType(__instance), IsValidDeathLinkLevel(__instance));
                }
            }
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
            static void Postfix(Level __instance, bool ___secretTriggered) {
                Logging.Log("_OnPreWin", LoggingFlags.Debug);
                APCheck(__instance, ___secretTriggered);
            }

            private static void APCheck(Level instance, bool secret = false) {
                Logging.Log("[LevelHook] APCheck");
                if (APData.IsCurrentSlotEnabled()) {
                    Logging.Log($"[LevelHook] Level: {instance.CurrentLevel}");
                    if (instance.CurrentLevel == Levels.Devil || instance.CurrentLevel == Levels.Saltbaker) {
                        Goals goalFlag = (instance.CurrentLevel == Levels.Saltbaker) ? Goals.Saltbaker : Goals.Devil;
                        Logging.Log($"[LevelHook] Goal: {goalFlag}");
                        APClient.GoalComplete(goalFlag, false);
                        if (!APClient.LocationExists(LevelLocationMap.GetLocationId(instance.CurrentLevel, 0))) {
                            Logging.Log("[LevelHook] Not Checking Final Location");
                            return;
                        }
                    }
                    if (instance.CurrentLevel == Levels.Graveyard) {
                        Logging.Log("[LevelHook] Not a checkable level");
                    }
                    else if (instance.CurrentLevel == Levels.Mausoleum) {
                        Logging.Log("[LevelHook] Mausoleum Type");
                        switch (PlayerData.Data.CurrentMap) {
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
                    else if (Level.IsChessBoss) {
                        Logging.Log("[LevelHook] Chess Castle");
                        Levels clevel = instance.CurrentLevel;
                        if (!IsChalice() || !APSettings.DLCChessChaliceChecks) {
                            APClient.Check(LevelLocationMap.GetLocationId(clevel, 0), false);
                        }
                        else if (IsChalice()) {
                            APClient.Check(LevelLocationMap.GetLocationId(clevel, 1), false);
                        }
                    }
                    else if (!Level.IsInBossesHub || instance.CurrentLevel == Levels.DicePalaceMain) {
                        switch (instance.LevelType) {
                            case Level.Type.Battle: {
                                    Logging.Log("[LevelHook] Battle Type");
                                    Level.Mode battleNormalMode = APSettings.Hard ? Level.Mode.Hard : Level.Mode.Normal;
                                    Logging.Log($"[LevelHook] Difficulty Test: {Level.Difficulty}>={battleNormalMode}");
                                    if (Level.Difficulty >= battleNormalMode) {
                                        Levels clevel = instance.CurrentLevel;
                                        bool vsecret = secret && APSettings.BossSecretChecks && (clevel == Levels.Veggies || clevel == Levels.FlyingGenie || clevel == Levels.SallyStagePlay);
                                        int ccheck = 0;
                                        // NOTE: Secrets are counted if playing as Chalice
                                        if (!IsChalice() || (APSettings.DLCBossChaliceChecks & DlcChaliceCheckModes.Seperate) == 0 || vsecret) {
                                            APClient.Check(LevelLocationMap.GetLocationId(clevel, vsecret ? 3 : 0), false);
                                        }
                                        if (IsChalice()) {
                                            if (APSettings.DLCBossChaliceChecks == DlcChaliceCheckModes.Enabled)
                                                ccheck |= 2;
                                            ccheck |= 1;
                                        }
                                        if (APSettings.BossGradeChecks > 0) {
                                            if (Level.Grade >= (LevelScoringData.Grade.AMinus + (((int)APSettings.BossGradeChecks) - 1))) {
                                                if (!IsChalice() || APSettings.DLCBossChaliceChecks != DlcChaliceCheckModes.GradeRequired)
                                                    APClient.Check(LevelLocationMap.GetLocationId(clevel, 1), false);
                                                if (IsChalice() && APSettings.DLCBossChaliceChecks != DlcChaliceCheckModes.Disabled)
                                                    ccheck |= 2;
                                            }
                                        }
                                        if (ccheck >= 3) {
                                            APClient.Check(LevelLocationMap.GetLocationId(clevel, 2), false);
                                        }
                                    }
                                    else {
                                        Logging.Log("[LevelHook] Difficulty needs to be higher for there to be checks");
                                    }
                                    break;
                                }
                            case Level.Type.Platforming: {
                                    Logging.Log("[LevelHook] Platforming Type");
                                    Logging.Log($"[LevelHook] Difficulty Test: {Level.Difficulty}>={Level.Mode.Normal}");
                                    if (Level.Difficulty >= Level.Mode.Normal) {
                                        Levels clevel = instance.CurrentLevel;
                                        int ccheck = 0;
                                        if (!IsChalice() || (APSettings.DLCRunGunChaliceChecks & DlcChaliceCheckModes.Seperate) == 0) {
                                            APClient.Check(LevelLocationMap.GetLocationId(clevel, 0), false);
                                        }
                                        if (IsChalice()) {
                                            if ((APSettings.DLCRunGunChaliceChecks & DlcChaliceCheckModes.GradeRequired) == 0)
                                                ccheck |= 2;
                                            ccheck |= 1;
                                        }
                                        if (APSettings.RungunGradeChecks > 0) {
                                            if (Level.Grade >= (LevelScoringData.Grade.AMinus + (((int)APSettings.RungunGradeChecks) - 1))) {
                                                if (!IsChalice() || (APSettings.DLCRunGunChaliceChecks & DlcChaliceCheckModes.GradeRequired) == 0)
                                                    APClient.Check(LevelLocationMap.GetLocationId(clevel, ((int)APSettings.RungunGradeChecks > 3) ? 2 : 1), false);
                                                if (IsChalice() && APSettings.DLCRunGunChaliceChecks != DlcChaliceCheckModes.Disabled)
                                                    ccheck |= 2;
                                            }
                                        }
                                        if (ccheck >= 3) {
                                            APClient.Check(LevelLocationMap.GetLocationId(clevel, 3), false);
                                        }
                                    }
                                    else {
                                        Logging.Log("[LevelHook] Difficulty needs to be higher for there to be checks");
                                    }
                                    break;
                                }
                            default: {
                                    Logging.Log("[LevelHook] Other Level Type");
                                    break;
                                }
                        }
                    }
                    else if (Level.IsDicePalace && APSettings.DicePalaceBossSanity) {
                        Logging.Log("[LevelHook] DicePalace Type");
                        Level.Mode battleNormalMode = APSettings.Hard ? Level.Mode.Hard : Level.Mode.Normal;
                        Logging.Log($"[LevelHook] Difficulty Test: {Level.Difficulty}>={battleNormalMode}");
                        if (Level.Difficulty >= battleNormalMode) {
                            Levels clevel = instance.CurrentLevel;
                            if (!IsChalice() || !APSettings.DLCDicePalaceChaliceChecks) {
                                APClient.Check(LevelLocationMap.GetLocationId(clevel, 0), false);
                            }
                            else if (IsChalice()) {
                                APClient.Check(LevelLocationMap.GetLocationId(clevel, 1), false);
                            }
                        }
                        else {
                            Logging.Log("[LevelHook] Difficulty needs to be higher for there to be checks");
                        }
                    }
                    else {
                        Logging.Log("[LevelHook] Not a checkable level");
                    }
                }
            }
            private static bool IsChalice() {
                return PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).charm == Charm.charm_chalice;
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
                if (APData.IsCurrentSlotEnabled() && APManager.Current != null)
                    APManager.Current.SetActive(false);
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_coinManager = typeof(PlayerData).GetField("coinManager", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_mode = typeof(Level).GetProperty("mode")?.GetGetMethod();
                MethodInfo _mi_get_LevelType = typeof(Level).GetProperty("LevelType")?.GetGetMethod();
                MethodInfo _mi_set_Difficulty = typeof(Level).GetProperty("Difficulty")?.GetSetMethod(true);
                MethodInfo _mi_HackDifficulty = typeof(zHack_OnWin).GetMethod("HackDifficulty", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_GetCoinCollected =
                    typeof(PlayerData.PlayerCoinManager).GetMethod("GetCoinCollected", BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null);
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_CurrentLevel = typeof(Level).GetProperty("CurrentLevel", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_APCoinCondition = typeof(zHack_OnWin).GetMethod("APCoinCondition", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APCoinCheck = typeof(zHack_OnWin).GetMethod("APCoinCheck", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APCheck = typeof(zHack_OnWin).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_skipcoin = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 8; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_mode &&
                        codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_set_Difficulty) {
                        List<CodeInstruction> ncodes = [
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Call, _mi_get_LevelType),
                            new CodeInstruction(OpCodes.Call, _mi_HackDifficulty)
                        ];
                        codes.InsertRange(i + 1, ncodes);
                        i += ncodes.Count;
                        success |= 1;
                    }
                    if ((success & 2) == 0) { //TODO Do Chalice checks
                        //List<CodeInstruction> ncodes = [];
                        //codes.InsertRange(i, ncodes);
                        //i+=ncodes.Count;
                        success |= 2;
                    }
                    if ((success & 4) == 0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i + 3].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i + 3].operand == _mi_AddCurrency && codes[i + 4].opcode == OpCodes.Call && (MethodInfo)codes[i + 4].operand == _mi_get_Data && codes[i + 7].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i + 7].operand == _mi_AddCurrency) {
                        codes[i + 4].labels.Add(l_skipcoin);
                        List<CodeInstruction> ncodes = [
                            //new CodeInstruction(OpCodes.Ldarg_0),
                            //new CodeInstruction(OpCodes.Callvirt, _mi_get_CurrentLevel),
                            //new CodeInstruction(OpCodes.Call, _mi_APCoinCheck),
                            CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brtrue, l_skipcoin),
                            ];
                        codes.InsertRange(i, ncodes);
                        i += ncodes.Count;
                        success |= 4;
                    }
                }
                if (success != 7) throw new Exception($"{nameof(zHack_OnWin)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static Level.Mode HackDifficulty(Level.Mode mode, Level.Type type) {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex) && type == Level.Type.Battle) {
                    return (APSettings.Hard && mode < Level.Mode.Hard) ? Level.Mode.Easy : mode;
                }
                else return mode;
            }
            private static bool APCoinCheck(Levels level) {
                if (APData.IsCurrentSlotEnabled()) {
                    long locationId = LevelLocationMap.GetLocationId(level, 0);
                    APCheck(locationId);
                    return true;
                }
                return false;
            }
            private static void APCheck(long loc) {
                if (!APClient.IsLocationChecked(loc))
                    APClient.Check(loc);
            }
        }

        private static APManager.MngrType GetLevelType(Level instance) {
            Logging.Log($"Level: {instance.CurrentLevel} | LevelType: {instance.LevelType} | Difficulty: {instance.mode}");
            return instance.LevelType switch {
                Level.Type.Battle or Level.Type.Platforming or Level.Type.Tutorial => true,
                _ => false,
            } ? instance.CurrentLevel switch {
                Levels.House or Levels.ShmupTutorial or Levels.DiceGate or Levels.Kitchen or Levels.ChessCastle => APManager.MngrType.Normal,
                Levels.Mausoleum => APManager.MngrType.SpecialLevel,
                _ => APManager.MngrType.Level,
            } : APManager.MngrType.Normal;
        }
        private static bool IsValidDeathLinkLevel(Level instance) {
            Logging.Log($"Level: {instance.CurrentLevel} | LevelType: {instance.LevelType} | Difficulty: {instance.mode}");
            return instance.LevelType switch {
                Level.Type.Battle or Level.Type.Platforming or Level.Type.Tutorial => true,
                _ => false,
            } && instance.CurrentLevel switch {
                Levels.House or Levels.ShmupTutorial or Levels.DiceGate or Levels.Kitchen or Levels.ChessCastle => false,
                _ => true,
            };
        }
    }
}

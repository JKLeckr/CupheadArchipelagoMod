using CupheadArchipelago.AP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class MapHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(Map), "Start")]
        internal static class Start {
            private static MethodInfo _mi_start_cr;

            static Start() {
                _mi_start_cr = typeof(Map).GetMethod("start_cr", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static bool Prefix() {
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;

                /*for (int i = 0; i < codes.Count; i++) {
                    Plugin.Log.LogInfo($"{codes[i].opcode}: {codes[i].operand}");
                }*/

                for (int i = 0; i < codes.Count - 2; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_start_cr) {
                        for (int j = i; i < codes.Count && codes[i].opcode != OpCodes.Ret; j++) {
                            codes.RemoveAt(i);
                        }
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Start)}: Patch Failed!");

                /*for (int i = 0; i < codes.Count; i++) {
                    Plugin.Log.LogInfo($"{codes[i].opcode}: {codes[i].operand}");
                }*/

                return codes;
            }
            static void Postfix(Map __instance, CupheadMapCamera ___cupheadMapCamera) {
                Plugin.Log(Level.Difficulty, LoggingFlags.Debug);
                __instance.StartCoroutine(_mi_start_cr.Name);
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) __instance.StartCoroutine(start_ap_cr(__instance));
            }

            private static IEnumerator start_ap_cr(Map __instance) {
                Console.WriteLine("[CupheadArchipelago] Map start_ap_cr");
                
                if (Level.Won) {
                    yield return CupheadTime.WaitForSeconds(__instance, 1.0f);

                    Level.Mode normalMode = APSettings.Hard?Level.Mode.Hard:Level.Mode.Normal;
                    
                    Plugin.Log(Level.PreviousLevel, LoggingFlags.Debug);

                    // TODO: true is there for debugging purposes, remove later
                    if (true || !Level.PreviouslyWon || Level.PreviousDifficulty < normalMode || Level.PreviousLevel == Levels.Mausoleum) {
                        if (!Level.IsDicePalace && !Level.IsDicePalaceMain && Level.PreviousLevel != Levels.Devil && Level.PreviousLevel != Levels.Saltbaker && 
                        Level.PreviousLevel != Levels.DicePalaceMain && Level.PreviousLevel != Levels.Mausoleum) {
                            switch (Level.PreviousLevelType) {
                                case Level.Type.Battle: {
                                    Plugin.Log("Battle", LoggingFlags.Debug);
                                    if (Level.Difficulty >= normalMode) {
                                        Plugin.Log(Level.Grade, LoggingFlags.Debug);
                                        Plugin.Log(APSettings.BossGradeChecks, LoggingFlags.Debug);
                                        APClient.Check(LevelLocationMap.GetLocationId(Level.PreviousLevel,0));
                                        if (APSettings.BossGradeChecks>0) {
                                            if (Level.Grade>=(LevelScoringData.Grade.AMinus+((int)APSettings.BossGradeChecks))) {
                                                APClient.Check(LevelLocationMap.GetLocationId(Level.PreviousLevel,1));
                                            }
                                        }
                                    }
                                    else Plugin.Log("Too Bahd", LoggingFlags.Debug);
                                    break;
                                }
                                case Level.Type.Platforming: {
                                    Plugin.Log("Platforming", LoggingFlags.Debug);
                                    Plugin.Log(Level.Grade, LoggingFlags.Debug);
                                    Plugin.Log(APSettings.RungunGradeChecks, LoggingFlags.Debug);
                                    APClient.Check(LevelLocationMap.GetLocationId(Level.PreviousLevel,0));
                                    if (APSettings.RungunGradeChecks>0) {
                                        if (Level.Grade>=(LevelScoringData.Grade.AMinus+((int)APSettings.RungunGradeChecks))) {
                                            APClient.Check(LevelLocationMap.GetLocationId(Level.PreviousLevel,((int)APSettings.RungunGradeChecks>3)?2:1));
                                        }
                                    }
                                    break;
                                }
                                default: {
                                    Plugin.Log("Other", LoggingFlags.Debug);
                                    break;
                                }
                            }
                        }
                    }
                }
                yield break;
            }
        }
    }
}
/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class LevelHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
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

        [HarmonyPatch(typeof(Level), "zHack_OnWin")]
        internal static class zHack_OnWin {
            static bool Prefix() {
                Plugin.Log("zHack_OnWin", LoggingFlags.Debug);
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                //bool debug = false;
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;
                MethodInfo _mi_get_mode = typeof(Level).GetProperty("mode").GetGetMethod();
                MethodInfo _mi_set_Difficulty = typeof(Level).GetProperty("Difficulty").GetSetMethod(true);

                /*if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                    Plugin.Log(_mi_get_mode);
                    Plugin.Log(_mi_set_Difficulty);
                }*/

                for (int i = 0; i < codes.Count - 2; i++) {
                    if (codes[i].opcode==OpCodes.Call && (MethodInfo)codes[i].operand==_mi_get_mode &&
                        codes[i+1].opcode==OpCodes.Call && (MethodInfo)codes[i+1].operand==_mi_set_Difficulty) {
                        codes.Insert(i+1, CodeInstruction.Call(typeof(zHack_OnWin), "HackDifficulty"));
                        //Plugin.Log("Patch success");
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(zHack_OnWin)}: Patch Failed!");
                //if (!success) Plugin.Log("Patch failed", BepInEx.Logging.LogLevel.Warning);
                /*if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }*/

                return codes;
            }

            private static Level.Mode HackDifficulty(Level.Mode mode) {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    return (APSettings.Hard&&mode<Level.Mode.Hard)?Level.Mode.Easy:mode;
                }
                else return mode;
            }
        }

        /*
        if (APData.CurrentSData.Hard&&Level.Difficulty==Level.Mode.Normal) {
                        _Difficulty = Level.Difficulty;
                        Plugin.Log.LogInfo("_pi_LevelDifficulty");
                        _pi_LevelDifficulty.SetValue(null, Level.Mode.Easy, null);
                    }
        */
    }
}
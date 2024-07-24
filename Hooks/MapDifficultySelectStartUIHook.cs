/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks {
    public class MapDifficultySelectStartUIHook {
        public static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(SetDifficultyAvailability));
            Harmony.CreateAndPatchAll(typeof(Next));
            Harmony.CreateAndPatchAll(typeof(UpdateCursor));
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "Awake")]
        internal static class Awake {
            static bool Prefix() {
                Level.SetCurrentMode(GetClamppedCurrentLevelMode());
                return true;
            }
            static void Postfix(int ___index) {
                Plugin.Log($"Index: {___index}", LoggingFlags.Debug);
            }

            private static Level.Mode GetClamppedCurrentLevelMode() {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    //Plugin.Log.LogInfo("Clamping mode");
                    if (APData.CurrentSData.Hard||Level.CurrentMode>0) {
                        return (Level.Mode)(APData.CurrentSData.Hard?2:1);
                    } else return 0;
                }
                else return Level.CurrentMode;
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "SetDifficultyAvailability")]
        internal static class SetDifficultyAvailability {
            //FIXME: Eventually do better with this code.
            static bool Prefix(ref int ___index, MapDifficultySelectStartUI __instance, ref Level.Mode[] ___options, 
            RectTransform ___easy, RectTransform ___normalSeparator, RectTransform ___normal, RectTransform ___hardSeparator, RectTransform ___hard) {
                if (!APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) return true;
                bool hardonly = false;
                if (APData.CurrentSData.Hard || PlayerData.Data.CurrentMap == Scenes.scene_map_world_4 || __instance.level == "Saltbaker") {
                    if (APData.CurrentSData.Hard)
                        if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4) {
                            ___options = new Level.Mode[1] {Level.Mode.Hard};
                            hardonly = true;
                        }
                        else {
                            ___options = new Level.Mode[2] {Level.Mode.Normal, Level.Mode.Hard};
                        }
                    else
                        ___options = new Level.Mode[1] {Level.Mode.Normal};
                    ___easy.gameObject.SetActive(false);
                    ___normalSeparator.gameObject.SetActive(false);
                }
                else {
                    ___options = new Level.Mode[2] {
                        Level.Mode.Easy,
                        Level.Mode.Normal
                    };
                    ___easy.gameObject.SetActive(true);
                    ___normalSeparator.gameObject.SetActive(true);
                }
                ___index = Mathf.Max(0, ___options.Length-1);
                ___normal.gameObject.SetActive(!hardonly);
                ___hardSeparator.gameObject.SetActive(APData.CurrentSData.Hard&&!hardonly);
                ___hard.gameObject.SetActive(APData.CurrentSData.Hard);
                return false;
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "Next")]
        internal static class Next {
            static bool Prefix(int ___index) {
                Plugin.Log($"[Next] Index: {___index}", LoggingFlags.Debug);
                return true;
            }
            static void Postfix(int ___index) {
                Plugin.Log($"[Next] New Index: {___index}", LoggingFlags.Debug);
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "UpdateCursor")]
        internal static class UpdateCursor {
            private static RectTransform _normal;
            private static RectTransform _hard;

            static bool Prefix(RectTransform ___normal, RectTransform ___hard) {
                if (_normal==null || _hard==null) {
                    _normal = ___normal;
                    _hard = ___hard;
                }
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;
                FieldInfo _fi_normal = typeof(MapDifficultySelectStartUI).GetField("normal", BindingFlags.NonPublic | BindingFlags.Instance);

                /*for (int i = 0; i < codes.Count; i++) {
                    Plugin.Log.LogInfo($"{codes[i].opcode}: {codes[i].operand}");
                }*/

                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_normal
                        && codes[i+2].opcode == OpCodes.Callvirt && codes[i+3].opcode == OpCodes.Stloc_1) {
                        codes.RemoveAt(i);
                        codes[i] = CodeInstruction.Call(() => GetDefaultDifficulty());
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateCursor)}: Patch Failed!");

                /*for (int i = 0; i < codes.Count; i++) {
                    Plugin.Log.LogInfo($"{codes[i].opcode}: {codes[i].operand}");
                }*/

                return codes;
            }

            private static RectTransform GetDefaultDifficulty() => (APData.IsCurrentSlotEnabled() && APData.CurrentSData.Hard)?_hard:_normal;
        }
    }
}
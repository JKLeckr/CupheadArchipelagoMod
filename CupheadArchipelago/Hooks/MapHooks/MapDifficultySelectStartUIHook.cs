/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapDifficultySelectStartUIHook {
        internal static void Hook() {
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
                Logging.Log($"Index: {___index}", LoggingFlags.Debug);
            }

            private static Level.Mode GetClamppedCurrentLevelMode() {
                if (APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) {
                    //Logging.Log.LogInfo("Clamping mode");
                    if (APSettings.Hard||Level.CurrentMode>0) {
                        return (Level.Mode)(APSettings.Hard?2:1);
                    } else return 0;
                }
                else return Level.CurrentMode;
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "CheckInput")]
        internal static class CheckInput {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;

                MethodInfo _mi_GetButtonDown = typeof(AbstractMapSceneStartUI).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i+1].operand == 13 &&
                        codes[i+2].opcode == OpCodes.Call && (MethodInfo)codes[i+2].operand == _mi_GetButtonDown && codes[i+3].opcode == OpCodes.Brfalse) {
                            Label end = (Label)codes[i+3].operand;
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Brfalse, end),
                                CodeInstruction.Call(() => APCanDoLevel()),
                            ];
                            codes.InsertRange(i+3, ncodes);
                            i+= ncodes.Length;
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(CheckInput)}: Patch Failed!");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log("---");
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APCanDoLevel() {
                if (APData.IsCurrentSlotEnabled()) {
                    //
                    return true;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "SetDifficultyAvailability")]
        internal static class SetDifficultyAvailability {
            //FIXME: Eventually do better with this code.
            static bool Prefix(ref int ___index, MapDifficultySelectStartUI __instance, ref Level.Mode[] ___options, 
            RectTransform ___easy, RectTransform ___normalSeparator, RectTransform ___normal, RectTransform ___hardSeparator, RectTransform ___hard) {
                if (!APData.IsSlotEnabled(PlayerData.CurrentSaveFileIndex)) return true;
                bool hardonly = false;
                if (APSettings.Hard || PlayerData.Data.CurrentMap == Scenes.scene_map_world_4 || __instance.level == "Saltbaker") {
                    if (APSettings.Hard)
                        if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4) {
                            ___options = [Level.Mode.Hard];
                            hardonly = true;
                        }
                        else {
                            ___options = [Level.Mode.Normal, Level.Mode.Hard];
                        }
                    else
                        ___options = [Level.Mode.Normal];
                    ___easy.gameObject.SetActive(false);
                    ___normalSeparator.gameObject.SetActive(false);
                }
                else {
                    ___options = [
                        Level.Mode.Easy,
                        Level.Mode.Normal
                    ];
                    ___easy.gameObject.SetActive(true);
                    ___normalSeparator.gameObject.SetActive(true);
                }
                ___index = Mathf.Max(0, ___options.Length-1);
                ___normal.gameObject.SetActive(!hardonly);
                ___hardSeparator.gameObject.SetActive(APSettings.Hard&&!hardonly);
                ___hard.gameObject.SetActive(APSettings.Hard);
                return false;
            }
        }

        [HarmonyPatch(typeof(MapDifficultySelectStartUI), "Next")]
        internal static class Next {
            static bool Prefix(int ___index) {
                Logging.Log($"[Next] Index: {___index}", LoggingFlags.Debug);
                return true;
            }
            static void Postfix(int ___index) {
                Logging.Log($"[Next] New Index: {___index}", LoggingFlags.Debug);
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
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_normal = typeof(MapDifficultySelectStartUI).GetField("normal", BindingFlags.NonPublic | BindingFlags.Instance);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
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
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static RectTransform GetDefaultDifficulty() => (APData.IsCurrentSlotEnabled() && APSettings.Hard)?_hard:_normal;
        }
    }
}

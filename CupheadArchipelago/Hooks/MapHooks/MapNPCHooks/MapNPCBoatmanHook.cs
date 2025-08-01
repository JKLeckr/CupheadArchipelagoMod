/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Config;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCBoatmanHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(Update));
            Harmony.CreateAndPatchAll(typeof(SelectWorld));
        }

        private static readonly long locationID = APLocation.dlc_cookie;
        private const sbyte DIALOGUER_VAR_ID = 22;

        private static bool boatEnabled = true;

        [HarmonyPatch(typeof(MapNPCBoatman), "Start")]
        internal static class Start {
            static bool Prefix(MapNPCBoatman __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (!APClient.APSessionGSPlayerData.dlc_boat) {
                        SetBoatActive(__instance, false);
                        boatEnabled = false;
                        Logging.Log($"Boat: {APClient.APSessionGSPlayerData.dlc_boat}");
                    }
                }
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;

                MethodInfo _mi_Dialoguer_SetGlobalFloat = typeof(Dialoguer).GetMethod("SetGlobalFloat", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_APConditionFloat = typeof(Start).GetMethod("APConditionFloat", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 10; i++) {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == DIALOGUER_VAR_ID && codes[i + 5].opcode == OpCodes.Brfalse &&
                        codes[i + 6].opcode == OpCodes.Ldc_I4_1 && codes[i + 7].opcode == OpCodes.Br && codes[i + 8].opcode == OpCodes.Ldc_I4_0 &&
                        codes[i + 9].opcode == OpCodes.Conv_R4 && codes[i + 10].opcode == OpCodes.Call && (MethodInfo)codes[i + 10].operand == _mi_Dialoguer_SetGlobalFloat) {
                        codes.Insert(i + 10, new CodeInstruction(OpCodes.Call, _mi_APConditionFloat));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Start)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            private static float APConditionFloat(float orig) {
                Logging.Log($"{nameof(MapNPCBoatman)}: {orig}");
                return orig;
            }
        }

        [HarmonyPatch(typeof(MapNPCBoatman), "Update")]
        internal static class Update {
            static void Postfix(MapNPCBoatman __instance) {
                if (!boatEnabled && APClient.APSessionGSPlayerData.dlc_boat) {
                    boatEnabled = true;
                    Logging.Log("Unlocked Boat");
                    SetBoatActive(__instance, true);
                }
            }
        }

        [HarmonyPatch(typeof(MapNPCBoatman), "SelectWorld")]
        internal static class SelectWorld {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                Type[] m_Cutscene_Load_args = [
                    typeof(Scenes),
                    typeof(Scenes),
                    typeof(SceneLoader.Transition),
                    typeof(SceneLoader.Transition),
                    typeof(SceneLoader.Icon),
                ];

                FieldInfo _fi_shouldShowChaliceTooltip = typeof(PlayerData).GetField("shouldShowChaliceTooltip", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_Gift = typeof(PlayerData).GetMethod("Gift", BindingFlags.Public | BindingFlags.Instance, null, [typeof(PlayerId), typeof(Charm)], null);
                MethodInfo _mi_Cutscene_Load = typeof(Cutscene).GetMethod("Load", BindingFlags.Public | BindingFlags.Static, null, m_Cutscene_Load_args, null);

                Label l_aftercookie = il.DefineLabel();
                Label l_afterload = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 11; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i + 2].opcode == OpCodes.Ldc_I4 &&
                        (int)codes[i + 2].operand == (int)Charm.charm_chalice && codes[i + 3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 3].operand == _mi_Gift &&
                        codes[i + 4].opcode == OpCodes.Call && (MethodInfo)codes[i + 4].operand == _mi_get_Data && codes[i + 6].opcode == OpCodes.Ldc_I4 &&
                        (int)codes[i + 6].operand == (int)Charm.charm_chalice && codes[i + 7].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 7].operand == _mi_Gift &&
                        codes[i + 8].opcode == OpCodes.Call && (MethodInfo)codes[i + 8].operand == _mi_get_Data && codes[i + 9].opcode == OpCodes.Ldc_I4_1 &&
                        codes[i + 10].opcode == OpCodes.Stfld && (FieldInfo)codes[i + 10].operand == _fi_shouldShowChaliceTooltip) {
                        List<Label> orig_labels = codes[i].labels;
                        codes[i].labels = [];
                        codes[i + 11].labels.Add(l_aftercookie);
                        CodeInstruction[] ncodes = [
                            CodeInstruction.Call(() => APCookieCondition()),
                                new CodeInstruction(OpCodes.Brfalse, l_aftercookie),
                            ];
                        codes.InsertRange(i, ncodes);
                        codes[i].labels = orig_labels;
                        i += ncodes.Length + 10;
                        success |= 1;
                    }
                    if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == (sbyte)Scenes.scene_level_kitchen &&
                        codes[i + 5].opcode == OpCodes.Call && (MethodInfo)codes[i + 5].operand == _mi_Cutscene_Load) {
                        List<Label> orig_labels = codes[i].labels;
                        codes[i].labels = [];
                        codes[i + 6].labels.Add(l_afterload);
                        CodeInstruction[] ncodes = [
                            CodeInstruction.Call(() => APLoadScene()),
                                new CodeInstruction(OpCodes.Brtrue, l_afterload),
                            ];
                        codes.InsertRange(i, ncodes);
                        codes[i].labels = orig_labels;
                        i += ncodes.Length + 5;
                        success |= 2;
                    }
                }
                if (success != 3) throw new Exception($"{nameof(Start)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            private static bool APCookieCondition() {
                if (APData.IsCurrentSlotEnabled() && APSettings.DLCChaliceMode != DlcChaliceModes.Vanilla) {
                    Logging.Log("Cookie: false");
                    if (APSettings.DLCChaliceMode == DlcChaliceModes.Randomized) {
                        APClient.Check(locationID, true);
                    }
                    return false;
                }
                Logging.Log("Cookie: true");
                return true;
            }
            private static bool APLoadScene() {
                if (MConf.IsSkippingCutscene(Cutscenes.DLCIntro)) {
                    SceneLoader.LoadScene(Scenes.scene_level_kitchen, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris, SceneLoader.Icon.Hourglass);
                    return true;
                }
                return false;
            }
        }

        private static void SetBoatActive(MapNPCBoatman instance, bool set) {
            MapDialogueInteraction mdi = instance.GetComponent<MapDialogueInteraction>();
            mdi.enabled = set;
            Transform boat = instance.transform.GetChild(1);
            boat.gameObject.SetActive(set);
        }
    }
}

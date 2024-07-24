/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using TMPro;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks {
    public class SlotSelectScreenHook {
        private static Transform APInfoText;
        private static Transform APConStatusText;
        private static bool _lockMenu = false;
        private static SlotSelectScreen _instance;

        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(SetState));
            Harmony.CreateAndPatchAll(typeof(UpdateOptionsMenu));
            Harmony.CreateAndPatchAll(typeof(UpdatePlayerSelect));
            Harmony.CreateAndPatchAll(typeof(UpdateConfirmDelete));
            Harmony.CreateAndPatchAll(typeof(EnterGame));
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "Awake")]
        internal static class Awake {
            static bool Prefix(SlotSelectScreen __instance) {
                _instance = __instance;
                return true;
            }
            static void Postfix(SlotSelectScreen __instance) {
                //Plugin.Log.LogInfo("Awake");
                //Plugin.Log.LogInfo(__instance.gameObject.name);
                //Plugin.Log.LogInfo("child: "+__instance.transform.GetChild(1).GetComponent<Canvas>().renderMode);
                //Plugin.Log.LogInfo("child: "+__instance.transform.GetChild(1).GetComponent<Canvas>().worldCamera);
                CreateAPInfoText(__instance.transform);
                CreateAPConStatusText(__instance.transform);
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "Update")]
        internal static class Update {
            static bool Prefix(SlotSelectScreen __instance) {
                return true;
            }
            /*static void Postfix(SlotSelectScreen __instance, int ____slotSelection) {
                slotSelection = ____slotSelection;
            }*/
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "SetState")]
        internal static class SetState {
            static bool Prefix(ref SlotSelectScreen.State state) {
                if (!(state==SlotSelectScreen.State.SlotSelect||state==SlotSelectScreen.State.ConfirmDelete||state==SlotSelectScreen.State.PlayerSelect)) {
                    SetAPConStatusText("");
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdateOptionsMenu")]
        internal static class UpdateOptionsMenu {
            static bool Prefix() {
                APInfoText.gameObject.SetActive(!Cuphead.Current.controlMapper.isOpen);
                return true;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdatePlayerSelect")]
        internal static class UpdatePlayerSelect {
            private static int _slotSelection;
            private static MethodInfo _mi_game_start_cr;
            private static MethodInfo _mi_SetState;
            private static Thread _ap_connect_thread = null;
            private static Thread _ap_setup_thread = null;
            private static bool? _ap_connection_result = null;
            private static bool? _ap_setup_result = null;

            static UpdatePlayerSelect() {
                _mi_game_start_cr = typeof(SlotSelectScreen).GetMethod("game_start_cr", BindingFlags.NonPublic | BindingFlags.Instance);
                _mi_SetState = typeof(SlotSelectScreen).GetMethod("SetState", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static bool Prefix(int ____slotSelection) {
                _slotSelection = ____slotSelection;
                return !_lockMenu;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;

                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log.LogInfo($"{code.opcode}: {code.operand}");
                }
                Plugin.Log.LogInfo($"--------------------------------------------");*/

                for (int i=0;i<codes.Count-5;i++) {
                    if (codes[i].opcode==OpCodes.Ldarg_0 && codes[i+1].opcode==OpCodes.Ldarg_0
                        && codes[i+2].opcode==OpCodes.Call && (MethodInfo)codes[i+2].operand==_mi_game_start_cr) {
                            codes.RemoveRange(i,5);
                            codes.Insert(i, CodeInstruction.Call(() => ConnectAndStart()));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdatePlayerSelect)}: Patch Failed!");

                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log.LogInfo($"{code.opcode}: {code.operand}");
                }*/

                return codes;
            }

            private static void ConnectAndStart() {
                //Plugin.Log.LogInfo(_instance);
                //Plugin.Log.LogInfo(__slotSelection);
                _ap_connection_result = null;
                _ap_setup_result = null;
                _ap_connect_thread = new Thread(() => _ap_connection_result = APClient.CreateAndStartArchipelagoSession(_slotSelection));

                if (APData.SData[_slotSelection].enabled) {
                    _lockMenu = true;
                    SetAPConStatusText("Connecting...");
                    _ap_connect_thread.Start();
                    IEnumerator coroutine = connect_and_start_cr();
                    _instance.StartCoroutine(coroutine);
                }
                else {
                    _instance.StartCoroutine(_mi_game_start_cr.Name, 0);
                }
            }

            private static IEnumerator connect_and_start_cr() {
                while (!_ap_connection_result.HasValue) {
                    yield return null;
                }
                if (!(bool)_ap_connection_result) {
                    SetAPConStatusText("Connection failed!");
                    _lockMenu = false;
                    _mi_SetState.Invoke(_instance, new object[]{SlotSelectScreen.State.PlayerSelect});
                    AudioManager.Play("level_menu_select");
                }
                else {
                    SetAPConStatusText("Connected!\nChecking...");
                    while (APClient.SlotData==null) {
                        yield return null;
                    }
                    //Plugin.Log($"{APClient.SlotData["game_content"]} {APClient.SlotData["expert_mode"]} {APClient.SlotData["boss_grade_checks"]}");
                    //TODO: Update this later
                    if (DLCManager.DLCEnabled()!=APClient.APSessionSData.UseDLC) { //!DLCManager.DLCEnabled()&&APClient.APSessionSData.UseDLC
                        Plugin.Log("[APClient] Content Mismatch! Cannot use a non-DLC client on a DLC Archipelago slot!", LogLevel.Error);
                        SetAPConStatusText("Connected!\nError: Content Mismatch!");
                        APClient.CloseArchipelagoSession();
                        SetAPConStatusText("Disconnected.\nError: Content Mismatch!");
                        _lockMenu = false;
                        _mi_SetState.Invoke(_instance, new object[]{SlotSelectScreen.State.PlayerSelect});
                        AudioManager.Play("level_menu_select");
                    }
                    SetAPConStatusText("Connected!\nSetting Up...");
                    if (APClient.APSessionSData.Hard) Level.SetCurrentMode(Level.Mode.Hard);
                    else Level.SetCurrentMode(Level.Mode.Normal);
                    _ap_setup_thread = new Thread(() => _ap_setup_result = APClient.SetupArchipelagoGame());
                    _ap_setup_thread.Start();
                    while (!_ap_setup_result.HasValue) {
                        yield return null;
                    }
                    if ((bool)_ap_setup_result) {
                        SetAPConStatusText("Connected!\nDone!");
                        _instance.StartCoroutine(_mi_game_start_cr.Name, 0);
                        _lockMenu = false;
                    }
                    else {
                        SetAPConStatusText("Connected!\nSetup failed!");
                        _lockMenu = false;
                        _mi_SetState.Invoke(_instance, new object[]{SlotSelectScreen.State.PlayerSelect});
                        AudioManager.Play("level_menu_select");
                    }
                }
                yield break;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdateConfirmDelete")]
        internal static class UpdateConfirmDelete {
            private static int _slotSelection;

            static bool Prefix(int ____slotSelection) {
                _slotSelection = ____slotSelection;
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;
                MethodInfo _mi_ClearSlot = typeof(PlayerData).GetMethod("ClearSlot", BindingFlags.Public | BindingFlags.Static);

                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log.LogInfo($"{code.opcode}: {code.operand}");
                }*/

                for (int i=0;i<codes.Count;i++) {
                    if (codes[i].opcode==OpCodes.Call && (MethodInfo)codes[i].operand==_mi_ClearSlot) {
                        codes.Insert(i+1, CodeInstruction.Call(() => APDataClearSlot()));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateConfirmDelete)}: Patch Failed!");

                return codes;
            }

            private static void APDataClearSlot() {
                APData.ResetData(_slotSelection);
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "EnterGame")]
        internal static class EnterGame {
            private static int _slotSelection;

            static bool Prefix(int ____slotSelection) {
                _slotSelection = ____slotSelection;
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool success = false;

                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log.LogInfo($"{code.opcode}: {code.operand}");
                }
                Plugin.Log.LogInfo("------------------------------------------------------");*/
                
                for (int i=0;i<codes.Count-6;i++) {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 50) {
                        /*for (int j=0;j<6;j++) {
                            Plugin.Log.LogInfo($"REMOVING {codes[i+j].opcode}: {codes[i+j].operand}");
                        }*/
                        codes.RemoveRange(i,6);
                        codes.Insert(i, CodeInstruction.Call(() => LoadSceneNewSave()));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(EnterGame)}: Patch Failed!");

                /*foreach (CodeInstruction code in codes) {
                    Plugin.Log.LogInfo($"{code.opcode}: {code.operand}");
                }*/

                return codes;
            }

            private static void LoadSceneNewSave() {
                Plugin.Log("Loading Scene", LoggingFlags.Debug);
                if (APData.SData[_slotSelection].enabled&&!Plugin.ConfigSkipIntro) {
                    Cutscene.Load(Scenes.scene_level_house_elder_kettle, Scenes.scene_cutscene_intro, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
                }
                else {
                    SceneLoader.LoadScene(Scenes.scene_level_house_elder_kettle, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
                }
            }
        }

        private static void CreateAPInfoText(Transform parent) {
            GameObject obj = new GameObject("APInfoText");
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1,1);
            rect.anchorMax = new Vector2(1,1);
            rect.offsetMin = new Vector2(-1000,0);
            rect.pivot = new Vector2(1,1);
            rect.position = new Vector3(625,350,-15);
            
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.TopRight;
            txt.OverflowMode = TextOverflowModes.Overflow;
            txt.color = Color.white;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontSize = 16;
            txt.text = "CupheadArchipelago "+PluginInfo.PLUGIN_VERSION;
            
            obj.layer = 5;

            if (parent!=null) {
                obj.transform.SetParent(parent.GetChild(1));
            }

            APInfoText = obj.transform;
        }

        private static void CreateAPConStatusText(Transform parent) {
            GameObject obj = new GameObject("APStatusText");
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0,1);
            rect.anchorMax = new Vector2(0,1);
            rect.offsetMax = new Vector2(1000,0);
            rect.pivot = new Vector2(0,1);
            rect.position = new Vector3(-620,350,-15);

            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.OverflowMode = TextOverflowModes.Overflow;
            txt.color = Color.white;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontWeight = 600;
            txt.fontSize = 22;
            txt.text = "";
            
            obj.layer = 5;

            if (parent!=null) {
                obj.transform.SetParent(parent.GetChild(1));
            }

            APConStatusText = obj.transform;
        }
        private static void SetAPConStatusText(string str) {
            if (APConStatusText!=null) {
                TextMeshProUGUI txt = APConStatusText.GetComponent<TextMeshProUGUI>();
                txt.SetText(str);
            }
            else Plugin.Log("Error: APStatus Text is NULL!", LogLevel.Error);
        }
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using TMPro;
using BepInEx.Logging;

namespace CupheadArchipelago.Hooks.MenuHooks {
    internal class SlotSelectScreenHook {
        private static Transform APInfoText;
        private static Transform APConStatusText;
        private static bool _cancelPlayerSelection = false;
        private static bool _lockMenu = false;
        private static SlotSelectScreen _instance;

        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(SetState));
            Harmony.CreateAndPatchAll(typeof(UpdateOptionsMenu));
            Harmony.CreateAndPatchAll(typeof(UpdateSlotSelect));
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
                /*if (!(state==SlotSelectScreen.State.SlotSelect||state==SlotSelectScreen.State.ConfirmDelete||state==SlotSelectScreen.State.PlayerSelect)) {
                    SetAPConStatusText("");
                }*/
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
            /*
                FIXME: I hate this! Static vars when extracting values are so cringe!
                They are all over the place from earlier parts of development.
                Now that I am much more familiar with the Transpiler, redo these parts.
            */
            private static int _slotSelection;
            private static CupheadInput.AnyPlayerInput _input;
            private static MethodInfo _mi_game_start_cr;
            private static MethodInfo _mi_SetState;
            private static int Status => APClient.SessionStatus;

            static UpdatePlayerSelect() {
                _mi_game_start_cr = typeof(SlotSelectScreen).GetMethod("game_start_cr", BindingFlags.NonPublic | BindingFlags.Instance);
                _mi_SetState = typeof(SlotSelectScreen).GetMethod("SetState", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static bool Prefix(int ____slotSelection, CupheadInput.AnyPlayerInput ___input) {
                _slotSelection = ____slotSelection;
                _input = ___input;
                return !_lockMenu;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                int success = 0;
                bool debug = false;

                MethodInfo _mi_GetButtonDown = typeof(SlotSelectScreen).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);

                Label cblock = il.DefineLabel();

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-5;i++) {
                    if ((success&1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldarg_0 &&
                        codes[i+2].opcode == OpCodes.Call && (MethodInfo)codes[i+2].operand == _mi_game_start_cr) {
                            codes.RemoveRange(i,5);
                            codes.Insert(i, CodeInstruction.Call(() => ConnectAndStart()));
                            success |= 1;
                            i++;
                    }
                    if ((success&2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldc_I4_S &&
                        (sbyte)codes[i+1].operand == (int)CupheadButton.Cancel && codes[i+2].opcode == OpCodes.Call &&
                        (MethodInfo)codes[i+2].operand == _mi_GetButtonDown && codes[i+3].opcode == OpCodes.Brfalse) {
                            Label cpass = (Label)codes[i+3].operand;
                            codes[i+4].labels.Add(cblock);
                            codes[i+3] = new CodeInstruction(OpCodes.Brtrue, cblock);
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => IsAPCancelPlayerSelection()),
                                new CodeInstruction(OpCodes.Brfalse, cpass),
                            ];
                            codes.InsertRange(i+4, ncodes);
                            i+=ncodes.Count;
                            success |= 2;
                    }
                }
                if (success!=3) throw new Exception($"{nameof(UpdatePlayerSelect)}: Patch Failed! {success}");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log("---");
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static void ConnectAndStart() {
                //Plugin.Log.LogInfo(_instance);
                //Plugin.Log.LogInfo(__slotSelection);

                if (APData.SData[_slotSelection].error>0) {
                    Plugin.LogError($"Bad file! E{APData.SData[_slotSelection].error}");
                    SetAPConStatusText("AP data Error!\nFailed to load!\nCheck Log!");
                    APAbort(false);
                    return;
                }

                if (APData.SData[_slotSelection].enabled) {
                    _lockMenu = true;
                    if (!APData.SData[_slotSelection].playerData.HasStartWeapon()) {
                        Plugin.Log("Cleaning APData...");
                        APData.ResetData(_slotSelection, false, false);
                    }
                    if (Status!=0) {
                        if (Status==1) {
                            SetAPConStatusText("Disconnecting...");
                            APClient.CloseArchipelagoSession(true);
                        }
                        else if (Status<0) {
                            Plugin.LogWarning($"Client Status is not reset correctly! S:{Status}");
                            APClient.ResetSessionError();
                        }
                        else {
                            Plugin.LogWarning($"Client Status is not reset correctly! S:{Status}");
                            APClient.CloseArchipelagoSession(true);
                        }
                    }
                    if (APData.SData[_slotSelection].version != APData.AP_DATA_VERSION) {
                        SetAPConStatusText("Error!\nData Version Mismatch!\nCheck Log!");
                        APAbort(false);
                        return;
                    }
                    SetAPConStatusText("Connecting...");
                    ThreadPool.QueueUserWorkItem(_ => APClient.CreateAndStartArchipelagoSession(_slotSelection));
                    _instance.StartCoroutine(connect_and_start_cr());
                }
                else {
                    _instance.StartCoroutine(_mi_game_start_cr.Name, 0);
                }
            }

            private static IEnumerator connect_and_start_cr() {
                int displayState = 0;
                while (!APClient.Ready) {
                    if (Status<0) {
                        APAbort();
                        yield break;
                    }
                    else if (Status>=3 && Status<5 && displayState!=3) {
                        SetAPConStatusText("Connected!\nChecking...");
                        displayState = 3;
                    }
                    else if (Status>=5 && displayState!=5) {
                        SetAPConStatusText("Connected!\nSetting Up...");
                        displayState = 5;
                    }
                    yield return null;
                }

                if (!APData.SData[_slotSelection].playerData.HasStartWeapon()) {
                    PlayerDataHook.APSanitizeSlot(_slotSelection);
                    APData.SData[_slotSelection].playerData.GotStartWeapon();
                }

                if (APSettings.Hard) Level.SetCurrentMode(Level.Mode.Hard);
                else Level.SetCurrentMode(Level.Mode.Normal);

                SetAPConStatusText("Connected!\nDone!");
                _instance.StartCoroutine(_mi_game_start_cr.Name, 0);
                _lockMenu = false;
            }
            private static void APAbort(bool displayError = true) {
                Plugin.Log($"Abort! Client Status {Status}");
                if (displayError) {
                    switch (Status) {
                        case -1: {
                            SetAPConStatusText("Connection failed!\nCheck Log!");
                            break;
                        }
                        case -3: {
                            SetAPConStatusText($"Disconnected!\nCheck failed!\nMultiworld Mismatch!");
                            break;
                        }
                        case -4: {
                            SetAPConStatusText($"Disconnected!\nCheck failed!\nContent Mismatch!");
                            break;
                        }
                        default: { 
                            SetAPConStatusText("Disconnected!\nError!\nCheck Log!");
                            break;
                        }
                    }
                }
                APClient.ResetSessionError();
                _cancelPlayerSelection = true;
            }
            private static void APErrorConnected(string message) {
                SetAPConStatusText("Connected!\n"+message);
                APClient.CloseArchipelagoSession();
                SetAPConStatusText("Disconnected.\n"+message);
                APAbort();
            }
            private static bool IsAPCancelPlayerSelection() {
                if (_cancelPlayerSelection) {
                    _cancelPlayerSelection = false;
                    _lockMenu = false;
                    return true;
                } else return false;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdateSlotSelect")]
        internal static class UpdateSlotSelect {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;
                
                FieldInfo _fi_slots = typeof(SlotSelectScreen).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi__slotSelection = typeof(SlotSelectScreen).GetField("_slotSelection", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_IsEmpty = typeof(SlotSelectScreenSlot).GetProperty("IsEmpty", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_GetButtonDown = typeof(SlotSelectScreen).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_IsAPEmpty = typeof(UpdateSlotSelect).GetMethod("IsAPEmpty", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-6;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_slots &&
                        codes[i+2].opcode == OpCodes.Ldarg_0 && codes[i+3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+3].operand == _fi__slotSelection &&
                        codes[i+4].opcode == OpCodes.Ldelem_Ref && codes[i+5].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+5].operand == _mi_get_IsEmpty &&
                        codes[i+6].opcode == OpCodes.Brtrue) {
                            codes[i+4] = new CodeInstruction(OpCodes.Call, _mi_IsAPEmpty);
                            codes.RemoveAt(i+5);
                            success++;
                            break;
                    }
                }
                if (success!=1) throw new Exception($"{nameof(SlotSelectScreen)}: Patch Failed! {success}");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log("---");
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool DT(int num) {
                Plugin.Log($"T:{num}");
                return true;
            }

            private static bool IsAPEmpty(SlotSelectScreenSlot[] slots, int slotnum) {
                return APData.IsSlotEnabled(slotnum) ? APData.IsSlotEmpty(slotnum) : slots[slotnum].IsEmpty;
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
            txt.color = UnityEngine.Color.white;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontSize = 16;
            txt.text = $"CupheadArchipelago {Plugin.Version}";
            
            obj.layer = 5;

            if (parent!=null) {
                obj.transform.SetParent(parent.GetChild(1));
            }

            APInfoText = obj.transform;
        }

        private static void CreateAPConStatusText(Transform parent) {
            //FIXME: Fix text not positioned correctly on some resolutions
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
            txt.color = UnityEngine.Color.white;
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
        private static string GetAPConStatusText() {
            if (APConStatusText!=null) {
                TextMeshProUGUI txt = APConStatusText.GetComponent<TextMeshProUGUI>();
                return txt.text;
            }
            else Plugin.Log("Error: APStatus Text is NULL!", LogLevel.Error);
            return "";
        }
    }
}

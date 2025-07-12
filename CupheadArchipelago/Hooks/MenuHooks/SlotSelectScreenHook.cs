/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CupheadArchipelago.Hooks.MenuHooks {
    internal class SlotSelectScreenHook {
        private static Transform apInfoText;
        private static Transform apConStatusText;
        private static Transform apPrompt;
        private static Transform apPrompt2;
        private static Transform apGlyph1;
        private static Transform apGlyph2;
        private static Transform apSpacer;
        private static APSetupMenu apSetupMenu;
        private static bool apSetupMenuState = false;
        private static bool _cancelPlayerSelection = false;
        private static bool _lockMenu = false;
        private static SlotSelectScreen _instance;

        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            //Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(SetState));
            Harmony.CreateAndPatchAll(typeof(Update));
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
            static void Postfix(SlotSelectScreen __instance, int ____slotSelection) {
                //Logging.Log.LogInfo("Awake");
                //Logging.Log.LogInfo(__instance.gameObject.name);
                //Logging.Log.LogInfo("child: "+__instance.transform.GetChild(1).GetComponent<Canvas>().renderMode);
                //Logging.Log.LogInfo("child: "+__instance.transform.GetChild(1).GetComponent<Canvas>().worldCamera);
                CreateAPInfoText(__instance.transform);
                CreateAPConStatusText(__instance.transform);
                CreateAPSetupMenu(__instance, ____slotSelection);
                CreateAPPrompt(__instance);
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "Start")]
        internal static class Start {
            static bool Prefix() {
                return true;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "SetState")]
        internal static class SetState {
            static bool Prefix() {
                if (apSetupMenuState) {
                    SetAPSetupMenuState(false);
                }
                return true;
            }
            static void Postfix(SlotSelectScreen.State state) {
                SetAPPromptsActive(state == SlotSelectScreen.State.SlotSelect);
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "Update")]
        internal static class Update {
            static void Postfix(SlotSelectScreen.State ___state) {
                if (___state != SlotSelectScreen.State.PlayerSelect && _cancelPlayerSelection) _cancelPlayerSelection = false;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdateOptionsMenu")]
        internal static class UpdateOptionsMenu {
            static bool Prefix() {
                apInfoText.gameObject.SetActive(!Cuphead.Current.controlMapper.isOpen);
                return true;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdatePlayerSelect")]
        internal static class UpdatePlayerSelect {
            /*
                FIXME: I hate this! Static vars when extracting values are so cringe!
                There should be a better way to do this!
            */
            private static int _slotSelection;
            private static MethodInfo _mi_game_start_cr;
            private static int Status => APClient.SessionStatus;

            static UpdatePlayerSelect() {
                _mi_game_start_cr = typeof(SlotSelectScreen).GetMethod("game_start_cr", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static bool Prefix(int ____slotSelection) {
                _slotSelection = ____slotSelection;
                return !_lockMenu;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                MethodInfo _mi_GetButtonDown = typeof(SlotSelectScreen).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);

                Label cblock = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
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
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => IsAPCancelPlayerSelection()),
                                new CodeInstruction(OpCodes.Brfalse, cpass),
                            ];
                            codes.InsertRange(i+4, ncodes);
                            i+=ncodes.Length;
                            success |= 2;
                    }
                }
                if (success!=3) throw new Exception($"{nameof(UpdatePlayerSelect)}: Patch Failed! {success}");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log("---");
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static void ConnectAndStart() {
                Logging.Log($"Starting slot {_slotSelection}");

                if (APData.SData[_slotSelection].state>0) {
                    Logging.LogError($"Bad file! E{APData.SData[_slotSelection].state}");
                    SetAPConStatusText("AP data Error!\nFailed to load!\nCheck Log!");
                    APAbort(false);
                    return;
                }

                if (APData.SData[_slotSelection].enabled) {
                    _lockMenu = true;
                    if (!APData.SData[_slotSelection].playerData.HasStartWeapon()) {
                        Logging.Log("Cleaning APData...");
                        APData.ResetData(_slotSelection, false, false, false);
                    }
                    if (Status!=0) {
                        if (Status==1) {
                            SetAPConStatusText("Disconnecting...");
                            APClient.CloseArchipelagoSession(true);
                        }
                        else if (Status<0) {
                            Logging.LogWarning($"Client Status is not reset correctly! S:{Status}");
                            APClient.ResetSessionError();
                        }
                        else {
                            Logging.LogWarning($"Client Status is not reset correctly! S:{Status}");
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
                    if (!APData.IsSlotEmpty(_slotSelection)) {
                        APData.ResetData(_slotSelection, true, true, false);
                        APData.Save(_slotSelection);
                    }
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
                    else if (Status>=3 && Status<4 && displayState!=3) {
                        SetAPConStatusText("Connected!\nChecking...");
                        displayState = 3;
                    }
                    else if (Status>=4 && Status<6 && displayState!=4) {
                        SetAPConStatusText("Connected!\nChecking...\nGettings Data...");
                        displayState = 4;
                    }
                    else if (Status>=6 && Status<7 && displayState!=6) {
                        SetAPConStatusText("Connected!\nGetting Data...");
                        displayState = 6;
                    }
                    else if (Status>=7 && displayState!=7) {
                        SetAPConStatusText("Connected!\nSetting Up...");
                        displayState = 7;
                    }
                    yield return null;
                }

                if (!APData.SData[_slotSelection].playerData.HasStartWeapon()) {
                    try {
                        PlayerDataHook.APSanitizeSlot(_slotSelection);
                    } catch (Exception e) {
                        Logging.Log($"Failed to set up save data! {e}");
                        APClient.CloseArchipelagoSession(true);
                        APAbort();
                    }
                    APData.SData[_slotSelection].playerData.GotStartWeapon();
                }

                if (APSettings.Hard) Level.SetCurrentMode(Level.Mode.Hard);
                else Level.SetCurrentMode(Level.Mode.Normal);

                if (!APSettings.UseDLC) DLCManagerHook.DisableDLC();

                SetAPConStatusText("Connected!\nDone!");
                _instance.StartCoroutine(_mi_game_start_cr.Name, 0);
                _lockMenu = false;
                yield break;
            }
            private static void APAbort(bool displayError = true) {
                Logging.Log($"Abort! Client Status {Status}");
                if (displayError) {
                    switch (Status) {
                        case -1: {
                            SetAPConStatusText("Connection failed!\nCheck Log!");
                            break;
                        }
                        case -2: {
                            SetAPConStatusText("Disconnected!\nCheck failed!\nInvalid Slot Data!\nCheck Log!");
                            break;
                        }
                        case -3: {
                            SetAPConStatusText($"Disconnected!\nCheck failed!\nWrong Slot Data Version!\nCheck Log!");
                            break;
                        }
                        case -5: {
                            SetAPConStatusText($"Disconnected!\nCheck failed!\nSeed Mismatch!");
                            break;
                        }
                        case -6: {
                            SetAPConStatusText($"Disconnected!\nGet Failed!\nCheck Log!");
                            break;
                        }
                        case -8: {
                            SetAPConStatusText($"Disconnected!\nSetup failed!\nCheck Log!");
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
                _lockMenu = false;
            }
            private static void APErrorConnected(string message) {
                SetAPConStatusText("Connected!\n"+message);
                APClient.CloseArchipelagoSession();
                SetAPConStatusText("Disconnected.\n"+message);
                APAbort();
            }
            private static bool IsAPCancelPlayerSelection() {
                if (_cancelPlayerSelection) {
                    AudioManager.Play("level_select");
                    _cancelPlayerSelection = false;
                    return true;
                } else return false;
            }
        }

        [HarmonyPatch(typeof(SlotSelectScreen), "UpdateSlotSelect")]
        internal static class UpdateSlotSelect {
            private static MethodInfo _mi_GetButtonDown = typeof(SlotSelectScreen).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);

            static bool Prefix(CupheadInput.AnyPlayerInput ___input, RectTransform ___deletePrompt, RectTransform ___deleteGlyph, RectTransform ___deleteSpacer) {
                if (apSetupMenuState) {
                    if ((___input.GetButtonDown(CupheadButton.Cancel) && !apSetupMenu.IsTyping()) || 
                        (apSetupMenu.IsBackSelected() && ___input.GetButtonDown(CupheadButton.Accept))) {
                            DeactivateAPSetupMenu(___deletePrompt, ___deleteGlyph, ___deleteSpacer);
                    }
                    return false;
                }
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;
                
                FieldInfo _fi_slots = typeof(SlotSelectScreen).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi__slotSelection = typeof(SlotSelectScreen).GetField("_slotSelection", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_input = typeof(SlotSelectScreen).GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_deletePrompt = typeof(SlotSelectScreen).GetField("deletePrompt", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_deleteGlyph = typeof(SlotSelectScreen).GetField("deleteGlyph", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_deleteSpacer = typeof(SlotSelectScreen).GetField("deleteSpacer", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_IsEmpty = typeof(SlotSelectScreenSlot).GetProperty("IsEmpty", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_GetButtonDown = typeof(SlotSelectScreen).GetMethod("GetButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_GetAPButtonDown = typeof(UpdateSlotSelect).GetMethod("GetAPButtonDown", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_ActivateAPSetupMenu = typeof(UpdateSlotSelect).GetMethod("ActivateAPSetupMenu", BindingFlags.NonPublic| BindingFlags.Static);
                MethodInfo _mi_IsAPEmpty = typeof(UpdateSlotSelect).GetMethod("IsAPEmpty", BindingFlags.NonPublic | BindingFlags.Static);

                Label lacceptif = il.DefineLabel();
                
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-6;i++) {
                    if ((success&1)==0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i+1].operand == (sbyte)CupheadButton.Accept &&
                        codes[i+2].opcode == OpCodes.Call && (MethodInfo)codes[i+2].operand == _mi_GetButtonDown && codes[i+3].opcode == OpCodes.Brfalse) {
                            codes[i].labels.Add(lacceptif);
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi_input),
                                new CodeInstruction(OpCodes.Call, _mi_GetAPButtonDown),
                                new CodeInstruction(OpCodes.Brfalse_S, lacceptif),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi_deletePrompt),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi_deleteGlyph),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi_deleteSpacer),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldfld, _fi__slotSelection),
                                new CodeInstruction(OpCodes.Call, _mi_ActivateAPSetupMenu),
                                new CodeInstruction(OpCodes.Ret),
                            ];
                            codes.InsertRange(i, ncodes);
                            i += ncodes.Length;
                            success |= 1;
                    }
                    else if ((success&2)==0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_slots &&
                        codes[i+2].opcode == OpCodes.Ldarg_0 && codes[i+3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+3].operand == _fi__slotSelection &&
                        codes[i+4].opcode == OpCodes.Ldelem_Ref && codes[i+5].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+5].operand == _mi_get_IsEmpty &&
                        codes[i+6].opcode == OpCodes.Brtrue) {
                            codes[i+4] = new CodeInstruction(OpCodes.Call, _mi_IsAPEmpty);
                            codes.RemoveAt(i+5);
                            success |= 2;
                    }
                    if (success>=7) break;
                }
                if (success!=3) throw new Exception($"{nameof(SlotSelectScreen)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool GetAPButtonDown(CupheadInput.AnyPlayerInput input) {
                return input.GetButton(CupheadButton.Lock) && input.GetButtonDown(CupheadButton.Accept);
            }
            private static void ActivateAPSetupMenu(RectTransform deletePrompt, RectTransform deleteGlyph, RectTransform deleteSpacer, int slotSelection) {
                if (apSetupMenu?.IsInitted() ?? false) {
                    AudioManager.Play("level_menu_select");
                    apSetupMenu.SetSlotSelection(slotSelection);
                    SetAPSetupMenuState(true);
                    deletePrompt.gameObject.SetActive(false);
                    deleteGlyph.gameObject.SetActive(false);
                    deleteSpacer.gameObject.SetActive(false);
                    SetAPPromptsActive(false);
                } else Logging.LogWarning("APSetupMenu is not loaded.");
            }
            private static void DeactivateAPSetupMenu(RectTransform deletePrompt, RectTransform deleteGlyph, RectTransform deleteSpacer) {
                AudioManager.Play("level_menu_select");
                SetAPSetupMenuState(false);
                deletePrompt.gameObject.SetActive(true);
                deleteGlyph.gameObject.SetActive(true);
                deleteSpacer.gameObject.SetActive(true);
                SetAPPromptsActive(true);
            }
            private static bool IsAPEmpty(SlotSelectScreenSlot[] slots, int slotnum) {
                //Logging.Log($"Prompt delete {slotnum}");
                return slots[slotnum].IsEmpty && APData.IsSlotEmpty(slotnum);
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
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_ClearSlot = typeof(PlayerData).GetMethod("ClearSlot", BindingFlags.Public | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count;i++) {
                    if (codes[i].opcode==OpCodes.Call && (MethodInfo)codes[i].operand==_mi_ClearSlot) {
                        codes.Insert(i+1, CodeInstruction.Call(() => APDataClearSlot()));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateConfirmDelete)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

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
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;
                
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-6;i++) {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 50) {
                        if (debug) {
                            for (int j=0;j<6;j++) {
                                Logging.Log($"REMOVING {codes[i+j].opcode}: {codes[i+j].operand}");
                            }
                        }
                        codes.RemoveRange(i,6);
                        codes.Insert(i, CodeInstruction.Call(() => LoadSceneNewSave()));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(EnterGame)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            static void Postfix() {
                APData.SData[_slotSelection].ResetLTime();
            }

            private static void LoadSceneNewSave() {
                Logging.Log("Loading Scene", LoggingFlags.Debug);
                if (!Config.IsSkippingCutscene(Cutscenes.Intro, APData.SData[_slotSelection].enabled)) {
                    Cutscene.Load(Scenes.scene_level_house_elder_kettle, Scenes.scene_cutscene_intro, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
                }
                else {
                    SceneLoader.LoadScene(Scenes.scene_level_house_elder_kettle, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
                }
            }
        }

        private static void SetAPSetupMenuState(bool state) {
            apSetupMenuState = state;
            apSetupMenu.SetState(state);
        }

        private static void CreateAPSetupMenu(SlotSelectScreen instance, int slotSelection) {
            Transform parent = instance.transform.GetChild(1);
            Transform prompts = parent.GetChild(8);
            
            GameObject obj = new GameObject("APSetupMenu");
            obj.SetActive(false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasGroup>();
            APSetupMenu apSM = obj.AddComponent<APSetupMenu>();

            instance.StartCoroutine(SetupAPSetupMenu_cr(apSM, parent, prompts, slotSelection));

            rect.SetParent(parent, false);

            obj.transform.SetSiblingIndex(8);

            apSetupMenu = apSM;
        }
        private static IEnumerator SetupAPSetupMenu_cr(APSetupMenu apSM, Transform parent, Transform prompts, int slotSelection) {
            Transform orig_options_p = parent.GetChild(5);
            while (orig_options_p.GetChildTransforms().Length<1) {
                yield return null;
            }
            Transform orig_options = orig_options_p.GetChild(0);
            while (!APData.Loaded) {
                yield return null;
            }
            apSM.SetSlotSelection(slotSelection);
            APSetupMenu.Init(apSM, orig_options, prompts);
            yield break;
        }

        private static void SetAPPromptsActive(bool active) {
            if (apPrompt==null || apPrompt2==null || apGlyph1==null || apGlyph2==null || apSpacer==null) return;
            apPrompt.gameObject.SetActive(active);
            apPrompt2.gameObject.SetActive(active);
            apGlyph1.gameObject.SetActive(active);
            apGlyph2.gameObject.SetActive(active);
            apSpacer.gameObject.SetActive(active);
        }
        private static void CreateAPPrompt(SlotSelectScreen instance) {
            Transform parent = instance.transform.GetChild(1);
            Transform orig_prompts = parent.transform.GetChild(9);
            int confirm_start_index = 1;
            Transform orig_confirmPrompt = orig_prompts.transform.GetChild(confirm_start_index);
            Transform orig_confirmGlyph = orig_prompts.transform.GetChild(confirm_start_index+1);
            Transform orig_confirmSpacer = orig_prompts.transform.GetChild(confirm_start_index+2);

            GameObject apPrompt = GameObject.Instantiate(orig_confirmPrompt.gameObject);
            apPrompt.name = "AP";
            apPrompt.transform.SetParent(orig_prompts);
            apPrompt.transform.SetSiblingIndex(confirm_start_index);
            UnityEngine.Object.Destroy(apPrompt.GetComponent<LocalizationHelper>());
            Text apPromptText = apPrompt.GetComponent<Text>();
            apPromptText.text = "ARCHIPELAGO";
            apPrompt.SetActive(false);
            SlotSelectScreenHook.apPrompt = apPrompt.transform;

            GameObject apGlyph1 = GameObject.Instantiate(orig_confirmGlyph.gameObject);
            apGlyph1.name = "APGlyph";
            apGlyph1.transform.SetParent(orig_prompts);
            apGlyph1.transform.SetSiblingIndex(confirm_start_index+1);
            CupheadGlyph apGlyph1Glyph = apGlyph1.GetComponent<CupheadGlyph>();
            apGlyph1Glyph.button = CupheadButton.Lock;
            apGlyph1Glyph.Init();
            apGlyph1.SetActive(false);
            SlotSelectScreenHook.apGlyph1 = apGlyph1.transform;

            GameObject apPrompt2 = GameObject.Instantiate(orig_confirmPrompt.gameObject);
            apPrompt2.name = "AP";
            apPrompt2.transform.SetParent(orig_prompts);
            apPrompt2.transform.SetSiblingIndex(confirm_start_index+2);
            UnityEngine.Object.Destroy(apPrompt2.GetComponent<LocalizationHelper>());
            Text apPrompt2Text = apPrompt2.GetComponent<Text>();
            apPrompt2Text.text = "+";
            apPrompt.SetActive(false);
            SlotSelectScreenHook.apPrompt2 = apPrompt2.transform;

            GameObject apGlyph2 = GameObject.Instantiate(orig_confirmGlyph.gameObject);
            apGlyph2.name = "APGlyph";
            apGlyph2.transform.SetParent(orig_prompts);
            apGlyph2.transform.SetSiblingIndex(confirm_start_index+3);
            CupheadGlyph apGlyph2Glyph = apGlyph2.GetComponent<CupheadGlyph>();
            apGlyph2Glyph.button = CupheadButton.Accept;
            apGlyph2Glyph.Init();
            apGlyph2.SetActive(false);
            SlotSelectScreenHook.apGlyph2 = apGlyph2.transform;

            GameObject apSpacer = GameObject.Instantiate(orig_confirmSpacer.gameObject);
            apSpacer.name = "APSpacer";
            apSpacer.transform.SetParent(orig_prompts);
            apSpacer.transform.SetSiblingIndex(confirm_start_index+4);
            apSpacer.SetActive(false);
            SlotSelectScreenHook.apSpacer = apSpacer.transform;
        }

        private static void CreateAPInfoText(Transform parent) {
            GameObject obj = new GameObject("APInfoText");
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            
            rect.anchorMin = new Vector2(1,1);
            rect.anchorMax = new Vector2(1,1);
            rect.pivot = new Vector2(1,1);
            
            rect.anchoredPosition = new Vector2(-25,-20);

            rect.sizeDelta = new Vector2(256f, 256f);
            
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.TopRight;
            txt.OverflowMode = TextOverflowModes.Overflow;
            txt.color = UnityEngine.Color.white;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontSize = 16;
            txt.text = $"CupheadArchipelago\n{Plugin.FullVersion}";
            
            obj.layer = 5;

            if (parent!=null) {
                obj.transform.SetParent(parent.GetChild(1), false);
            }

            apInfoText = obj.transform;
        }

        private static void CreateAPConStatusText(Transform parent) {
            GameObject obj = new GameObject("APStatusText");
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            
            rect.anchorMin = new Vector2(0,1);
            rect.anchorMax = new Vector2(0,1);
            rect.pivot = new Vector2(0,1);
            
            rect.anchoredPosition = new Vector2(25,-20);

            rect.sizeDelta = new Vector2(256f, 512f);

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
                obj.transform.SetParent(parent.GetChild(1), false);
            }

            apConStatusText = obj.transform;
        }
        private static void SetAPConStatusText(string str) {
            if (apConStatusText!=null) {
                TextMeshProUGUI txt = apConStatusText.GetComponent<TextMeshProUGUI>();
                txt.SetText(str);
            }
            else Logging.LogError("Error: APStatus Text is NULL!");
        }
        private static string GetAPConStatusText() {
            if (apConStatusText!=null) {
                TextMeshProUGUI txt = apConStatusText.GetComponent<TextMeshProUGUI>();
                return txt.text;
            }
            else Logging.LogError("Error: APStatus Text is NULL!");
            return "";
        }
    }
}

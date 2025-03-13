/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago;
using CupheadArchipelago.AP;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Unity {
    public class APSetupMenu : MonoBehaviour {
        private CupheadInput.AnyPlayerInput input;
        private bool initted = false;
        private bool active = false;
        private int menuSelection = 0;
        private Text[] menuText;
        private bool menuLocked;

        private bool promptCooldown = false;
        
        private Transform fader;
        private APTypingPrompt typingPrompt;

        private int slotSelection = 0;
        private APData apData;

        private float menuTime = 0f;

        private static string[] setupFieldLabels = {
            "ENABLED",
            "ADDRESS",
            "PORT",
            "PLAYER",
            "PASSWORD",
        };

        void Awake() {
            input = new CupheadInput.AnyPlayerInput(false);
            menuTime = 0f;
        }

        void Update() {
            if (!initted || !active) return;
            if (typingPrompt.IsActive()) {
                if (typingPrompt.IsFinished()) {
                    switch (menuSelection) {
                        case 1: {
                            apData.address = typingPrompt.GetText();
                            break;
                        }
                        case 2: {
                            try {
                                int res;
                                res = int.Parse(typingPrompt.GetText());
                                apData.port = res;
                            } catch (Exception e) {
                                Logging.LogWarning($"[APSetupMenu] Invalid text for port: {e.Message}");
                            }
                            break;
                        }
                        case 3: {
                            apData.player = typingPrompt.GetText();
                            break;
                        }
                        case 4: {
                            apData.password = typingPrompt.GetText();
                            break;
                        }
                    }
                    CloseTypingPrompt();
                }
                return;
            }
            if (promptCooldown) {
                promptCooldown = false;
                return;
            }
            if (menuSelection == 0 && !menuLocked) {
                if (input.GetButtonDown(CupheadButton.MenuLeft) || input.GetButtonDown(CupheadButton.MenuRight)) {
                    AudioManager.Play("level_menu_select");
                    apData.enabled = !apData.enabled;
                    RefreshSettingsText();
                }
            }
            else if (menuSelection == 1 && input.GetButtonDown(CupheadButton.Accept)) {
                OpenTypingPrompt(apData.address, APTypingPrompt.TextTypes.Text);
            }
            else if (menuSelection == 2 && input.GetButtonDown(CupheadButton.Accept)) {
                OpenTypingPrompt(apData.port.ToString(), APTypingPrompt.TextTypes.SixDigit);
            }
            else if (menuSelection == 3 && input.GetButtonDown(CupheadButton.Accept)) {
                OpenTypingPrompt(apData.player, APTypingPrompt.TextTypes.Text16);
            }
            else if (menuSelection == 4 && input.GetButtonDown(CupheadButton.Accept)) {
                OpenTypingPrompt(apData.password, APTypingPrompt.TextTypes.Text16);
            }
            if (menuTime >= 0.15f) {
                if (input.GetButtonDown(CupheadButton.MenuUp)) {
                    menuTime = 0;
                    AudioManager.Play("level_menu_move");
                    menuSelection--;
                    if (menuLocked && menuSelection == 0) menuSelection = -1;
                    ClampMenuSelection();
                    SetSettingsTextColors();
                }
                else if (input.GetButtonDown(CupheadButton.MenuDown)) {
                    menuTime = 0;
                    AudioManager.Play("level_menu_move");
                    menuSelection++;
                    if (menuSelection>=menuText.Length) menuSelection = 0;
                    ClampMenuSelection();
                    SetSettingsTextColors();
                }
            } else {
                menuTime += Time.deltaTime;
            }
        }

        private void OpenTypingPrompt(string initial_str, APTypingPrompt.TextTypes type) {
            //fader.gameObject.SetActive(false);
            promptCooldown = true;
            typingPrompt.OpenPrompt(initial_str, type);
        }
        private void CloseTypingPrompt() {
            //fader.gameObject.SetActive(true);
            typingPrompt.ClosePrompt();
        }

        private void SetSettingsTextColors() {
            for (int i=0;i<menuText.Length;i++) {
                if (i==0 && menuLocked)
                    menuText[i].color = APCore.TEXT_INACTIVE_COLOR;
                else
                    menuText[i].color = (menuSelection == i) ? APCore.TEXT_SELECT_COLOR : APCore.TEXT_COLOR;
            }
        }

        private void RefreshAvailableText() {
            if (APData.IsSlotEmpty(slotSelection, true)) menuLocked = false;
        }

        private void ClampMenuSelection() {
            if (menuSelection<0) menuSelection = menuText.Length - 1;
            if (menuSelection>=menuText.Length) menuSelection = menuLocked ? 0 : 1;
        }

        public bool IsBackSelected() => menuSelection == 5;
        public bool IsTyping() => typingPrompt?.IsTyping() ?? false;

        public void SetState(bool state) {
            gameObject.SetActive(state);
            active = state;
            CloseTypingPrompt();
            if (state) {
                RefreshMenu();
            }
            menuSelection = menuLocked ? 1 : 0;
        }
        public void SetSlotSelection(int slotSelection) {
            apData = APData.SData[slotSelection];
        }

        public bool IsInitted() => initted;

        public static void Init(APSetupMenu instance, Transform orig_options) {
            Transform obj = instance.gameObject.transform;
            Transform orig_fader = orig_options.GetChild(0);
            Transform orig_card = orig_options.GetChild(1);
            Transform orig_bigcard = orig_card.GetChild(2);
            Transform orig_visualmenu = orig_card.GetChild(3);
            Transform orig_bignoise = orig_card.GetChild(6);

            GameObject fader = GameObject.Instantiate(orig_fader.gameObject, obj);
            fader.name = orig_fader.name;
            RectTransform fader_rect = fader.GetComponent<RectTransform>();
            fader_rect.sizeDelta = new Vector2(2570f, 1450f);
            instance.fader = fader.transform;

            GameObject card = new GameObject("Card");
            RectTransform card_rect = card.AddComponent<RectTransform>();
            card_rect.sizeDelta = new Vector2(514f, 299f);
            card.transform.SetParent(obj);

            GameObject bigcard = Instantiate(orig_bigcard.gameObject, card.transform);
            bigcard.name = orig_bigcard.name;
            bigcard.SetActive(true);
            
            GameObject menu = new GameObject("APMenu");
            RectTransform menu_rect = menu.AddComponent<RectTransform>();
            menu.AddComponent<HorizontalLayoutGroup>();
            menu_rect.sizeDelta = new Vector2(514f, 299f);
            menu.transform.SetParent(card.transform);

            GameObject settingsLabels = new GameObject("SettingsLabels");
            RectTransform sl_rect = settingsLabels.AddComponent<RectTransform>();
            sl_rect.sizeDelta = new Vector2(514f, 259f);
            settingsLabels.AddComponent<VerticalLayoutGroup>();
            settingsLabels.transform.SetParent(menu.transform);
            instance.SetupSettingsLabels(settingsLabels.transform);

            instance.menuText = new Text[6];
            instance.menuLocked = false;

            GameObject settingsText = new GameObject("SettingsText");
            RectTransform st_rect = settingsText.AddComponent<RectTransform>();
            st_rect.sizeDelta = new Vector2(514f, 259f);
            settingsText.AddComponent<VerticalLayoutGroup>();
            settingsText.transform.SetParent(menu.transform);
            instance.SetupSettingsText(settingsText.transform);
            
            GameObject bignoise = Instantiate(orig_bignoise.gameObject, card.transform);
            bignoise.name = orig_bignoise.name;
            bignoise.SetActive(true);

            instance.typingPrompt = APTypingPrompt.CreateTypingPrompt(card.transform, orig_options);

            instance.RefreshMenu();

            instance.initted = true;
            Logging.Log("APSetupMenu Initialized");
        }

        private void RefreshMenu() {
            RefreshAvailableText();
            SetSettingsTextColors();
            RefreshSettingsText();
        }

        private void RefreshSettingsText() {
            if (menuText[0] != null) menuText[0].text = apData.enabled ? "ON" : "OFF";
            if (menuText[1] != null) menuText[1].text = $"[{GetMenuString(apData.address)}]" ?? "[]";
            if (menuText[2] != null) menuText[2].text = $"[{apData.port}]";
            if (menuText[3] != null) menuText[3].text = $"[{GetMenuString(apData.player)}]" ?? "[]";
            if (menuText[4] != null) {
                string chars = new('*', Mathf.Min(apData.password?.Length ?? 0, 16));
                menuText[4].text = $"[{chars}]";
            }
        }

        private static string GetMenuString(string str) {
            if (str.Length>14) {
                return $"{str.Substring(str.Length-3)}...";
            }
            return str;
        }

        private void SetupSettingsText(Transform parent) {
            GameObject nobj = new GameObject("Enabled");
            RectTransform rect = nobj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 40f);
            nobj.transform.SetParent(parent.transform);
            nobj.AddComponent<CanvasRenderer>();
            Text txt = CreateSettingsTextComponent(nobj);
            txt.text = "ON";
            menuText[0] = txt;

            GameObject nobj2 = new GameObject("Address");
            RectTransform rect2 = nobj2.AddComponent<RectTransform>();
            rect2.sizeDelta = new Vector2(600f, 40f);
            nobj2.transform.SetParent(parent.transform);
            nobj2.AddComponent<CanvasRenderer>();
            Text txt2 = CreateSettingsTextComponent(nobj2);
            txt2.text = "[ARCHIPELAGOGGG]";
            menuText[1] = txt2;

            GameObject nobj3 = new GameObject("Port");
            RectTransform rect3 = nobj3.AddComponent<RectTransform>();
            rect3.sizeDelta = new Vector2(600f, 40f);
            nobj3.transform.SetParent(parent.transform);
            nobj3.AddComponent<CanvasRenderer>();
            Text txt3 = CreateSettingsTextComponent(nobj3);
            txt3.text = "[38281]";
            menuText[2] = txt3;

            GameObject nobj4 = new GameObject("Player");
            RectTransform rect4 = nobj4.AddComponent<RectTransform>();
            rect4.sizeDelta = new Vector2(600f, 40f);
            nobj4.transform.SetParent(parent.transform);
            nobj4.AddComponent<CanvasRenderer>();
            Text txt4 = CreateSettingsTextComponent(nobj4);
            txt4.text = "[Player]";
            menuText[3] = txt4;

            GameObject nobj5 = new GameObject("Password");
            RectTransform rect5 = nobj5.AddComponent<RectTransform>();
            rect5.sizeDelta = new Vector2(600f, 40f);
            nobj5.transform.SetParent(parent.transform);
            nobj5.AddComponent<CanvasRenderer>();
            Text txt5 = CreateSettingsTextComponent(nobj5);
            txt5.text = "[*****]";
            menuText[4] = txt5;

            GameObject nobjb = new GameObject("Back");
            RectTransform rectb = nobjb.AddComponent<RectTransform>();
            rectb.sizeDelta = new Vector2(514f, 259f);
            nobjb.transform.SetParent(parent.transform);
            nobjb.AddComponent<CanvasRenderer>();
            Text txtb = CreateSettingsTextComponent(nobjb, true, TextAnchor.LowerCenter);
            txtb.text = "__BACK____________________________";
            menuText[5] = txtb;
        }
        private void SetupSettingsLabels(Transform parent) {
            foreach (string s in setupFieldLabels) {
                GameObject nobj = new GameObject(s);
                RectTransform nrect = nobj.AddComponent<RectTransform>();
                nrect.sizeDelta = new Vector2(600f, 40f);
                nobj.transform.SetParent(parent.transform);
                nobj.AddComponent<CanvasRenderer>();
                Text txt = CreateSettingsTextComponent(nobj, true, TextAnchor.UpperRight);
                txt.text = $"{s}: ";
            }
            GameObject blank = new GameObject("Blank");
            RectTransform rect = blank.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 40f);
            blank.transform.SetParent(parent.transform);
            blank.AddComponent<CanvasRenderer>();
            CreateSettingsTextComponent(blank, true, TextAnchor.UpperRight).text = " ";
        }

        private static Text CreateSettingsTextComponent(GameObject obj, bool bold = false, TextAnchor alignment = TextAnchor.UpperLeft) {
            APCore.FontType type = bold ? APCore.FontType.ExtraBold : APCore.FontType.MonoSpace;
            return APCore.CreateSettingsTextComponent(obj, type, alignment, true);
        }
    }
}

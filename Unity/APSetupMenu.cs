/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago;
using CupheadArchipelago.AP;
using CupheadArchipelago.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Unity {
    public class APSetupMenu : MonoBehaviour {
        private bool initted = false;
        private bool active = false;
        private int slotSelection = 0;
        private CupheadInput.AnyPlayerInput input;

        [SerializeField]
        private string[] setupFields = {
            "ENABLED:",
            "ADDRESS:",
            "PORT:",
            "PLAYER:",
            "PASSWORD:",
        };

        void Awake() {
            input = new CupheadInput.AnyPlayerInput(false);
        }

        void Update() {
            if (!active) return;
        }

        public void SetState(bool state) {
            gameObject.SetActive(state);
            active = state;
        }
        public void SetSlotSelection(int slotSelection) {
            this.slotSelection = slotSelection;
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

            GameObject card = new GameObject("Card");
            RectTransform card_rect = card.AddComponent<RectTransform>();
            card.transform.SetParent(obj);

            GameObject bigcard = Instantiate(orig_bigcard.gameObject, card.transform);
            bigcard.name = orig_bigcard.name;
            bigcard.SetActive(true);
            
            GameObject menu = new GameObject("APMenu");
            menu.transform.SetParent(obj);
            GameObject settingsText = new GameObject("SettingsText");
            settingsText.AddComponent<RectTransform>();
            settingsText.AddComponent<VerticalLayoutGroup>();
            //Settings
            GameObject settingsLabels = new GameObject("SettingsLabels");
            settingsLabels.AddComponent<RectTransform>();
            settingsLabels.AddComponent<VerticalLayoutGroup>();
            instance.SetupSettingsLabels(settingsLabels.transform);
            
            GameObject bignoise = Instantiate(orig_bignoise.gameObject, card.transform);
            bignoise.name = orig_bignoise.name;
            bignoise.SetActive(true);

            instance.initted = true;
            Logging.Log("APSetupMenu Initialized");
        }

        private void SetupSettingsLabels(Transform parent) {
            foreach (string s in setupFields) {
                GameObject nobj = new GameObject("Enabled");
                RectTransform rect = nobj.AddComponent<RectTransform>();
                nobj.transform.SetParent(parent.transform);
                nobj.AddComponent<CanvasRenderer>();
                Text txt = nobj.AddComponent<Text>();
                txt.color = new Color(0.212f, 0.212f, 0.212f);
                txt.font = FontLoader.GetFont(FontLoader.FontType.CupheadVogue_Bold_merged);
                txt.fontSize = 32;
                txt.text = s;
                txt.alignment = TextAnchor.UpperRight;
            }
        }
    }
}

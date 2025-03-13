/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago;
using CupheadArchipelago.AP;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Unity {
    public class APTypingPrompt : MonoBehaviour {
        private bool initted = false;
        private bool active = false;

        public enum TextTypes {
            Text,
            Text16,
            SixDigit
        }
        
        [SerializeField]
        private float typeDelay = 2f;
        private float typeTime = 0f;
        private bool typing = false;
        private TextTypes type;
        private string text = "";

        private Text textField;

        void Update() {
            if (!initted || !active) return;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                //typing = false;
                ClosePrompt();
            }
            else if (Input.GetKeyDown(KeyCode.Return)) {
                typing = false;
                text = textField.text;
            }
            if (typeTime >= typeDelay) {}
            else {
                typeTime += Time.deltaTime;
            }
        }

        public void OpenPrompt(string initial_str, TextTypes type = TextTypes.Text) {
            text = initial_str;
            this.type = type;
            typing = true;
            SetState(true);
        }
        public void ClosePrompt() {
            typing = false;
            SetState(false);
        }

        private void SetState(bool state) {
            gameObject.SetActive(state);
            active = state;
        }

        public bool IsInitted() => initted;
        public bool IsTyping() => typing;
        public bool IsActive() => active;
        public bool IsFinished() => active && !typing;
        public string GetText() => text;

        public static APTypingPrompt CreateTypingPrompt(Transform parent, Transform orig_options, Transform orig_fader = null) {
            GameObject obj = new GameObject("TypingPrompt");
            obj.SetActive(false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(514f, 299f);
            APTypingPrompt prompt = obj.AddComponent<APTypingPrompt>();
            rect.transform.SetParent(parent);

            if (orig_fader != null) {
                GameObject fader = Instantiate(orig_fader.gameObject, obj.transform);
                fader.name = orig_fader.name;
                RectTransform fader_rect = fader.GetComponent<RectTransform>();
                fader_rect.sizeDelta = new Vector2(2570f, 1450f);
            }

            Init(prompt, orig_options);

            return prompt;            
        }

        protected static void Init(APTypingPrompt instance, Transform orig_options) {
            Transform obj = instance.gameObject.transform;
            
            Transform orig_card = orig_options.GetChild(1);
            Transform orig_noise = orig_card.GetChild(1);

            GameObject ncard = Instantiate(orig_card.gameObject, obj.transform);
            ncard.name = "Card";

            GameObject tprompt = new GameObject("Prompt");
            RectTransform menu_rect = tprompt.AddComponent<RectTransform>();
            tprompt.AddComponent<VerticalLayoutGroup>();
            menu_rect.sizeDelta = new Vector2(514f, 299f);
            tprompt.transform.SetParent(obj.transform);

            GameObject header = new GameObject("Header");
            RectTransform hrect = header.AddComponent<RectTransform>();
            hrect.sizeDelta = new Vector2(600f, 40f);
            header.transform.SetParent(obj.transform);
            header.AddComponent<CanvasRenderer>();
            Text htxt = APCore.CreateSettingsTextComponent(header, APCore.FontType.Bold, TextAnchor.UpperCenter, true);
            htxt.fontSize = 26;
            htxt.text = "Press [Enter] to Accept, Press [Esc] to Cancel. Enter Text:";

            GameObject fobj = new GameObject("Field");
            RectTransform rect = fobj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 40f);
            fobj.transform.SetParent(obj.transform);
            fobj.AddComponent<CanvasRenderer>();
            Text txt = APCore.CreateSettingsTextComponent(fobj, APCore.FontType.MonoSpace, TextAnchor.MiddleCenter, false);
            txt.fontSize = 24;
            txt.text = new string('A', 255);
            instance.textField = txt;

            GameObject nnoise = Instantiate(orig_noise.gameObject, obj.transform);
            nnoise.name = "Noise";

            instance.initted = true;
            Logging.Log("APTypingPrompt Initialized");
        }
    }
}

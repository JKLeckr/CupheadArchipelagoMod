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
        private float cursorBlink = 0.25f;
        [SerializeField]
        private float typeDelay = 2f;
        [SerializeField]
        private float typeHoldDelay = 5f;
        private float cBlinkTime = 0f;
        private float typeTime = 0f;
        private float typeHoldTime = 0f;
        private bool cursorState = false;
        private bool typing = false;
        private TextTypes type;
        private string text = "";
        private string _text = "";

        private Text textField;
        private InputField inputField;

        private const int TEXT_MAX_LENGTH = 340;

        void Update() {
            if (!initted || !active) return;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ClosePrompt();
            }
            else if (Input.GetKeyDown(KeyCode.Return)) {
                StopTyping(true);
            }
            /*if (typing) {
                if (cBlinkTime >= cursorBlink) {
                    ToggleCursorState();
                    cBlinkTime = 0;
                }
                else {
                    cBlinkTime += Time.deltaTime;
                }
                if (typeTime < typeDelay) {
                    typeTime += Time.deltaTime;
                }
            }*/
        }

        public void OpenPrompt(string initial_str, TextTypes type = TextTypes.Text) {
            if (!initted) throw new System.Exception("Not initialized!");
            text = ClampText(initial_str);
            SetInputType(type);
            UpdateText(text);
            inputField.text = _text;
            SetState(true);
            StartTyping();
        }
        public void StartTyping() {
            if (active && !typing) {
                typing = true;
                //SetCursorState(true);
                inputField.ActivateInputField();
            }
        }
        public void StopTyping(bool saveText = true) {
            if (active && typing) {
                typing = false;
                inputField.DeactivateInputField();
                if (saveText) text = _text;
            }
        }
        public void ClosePrompt() {
            if (typing) StopTyping(false);
            SetState(false);
        }

        private void SetInputType(TextTypes type) {
            this.type = type;
            inputField.characterLimit = type switch {
                TextTypes.Text16 => 16,
                TextTypes.SixDigit => 6,
                _ => TEXT_MAX_LENGTH,
            };
            if (type == TextTypes.SixDigit) {
                inputField.contentType = InputField.ContentType.IntegerNumber;
            } else {
                inputField.contentType = InputField.ContentType.Standard;
            }
        }

        private void SetState(bool state) {
            gameObject.SetActive(state);
            active = state;
        }
        private void SetCursorState(bool set) {
            if (cursorState != set) {
                if (set) {
                    textField.text = _text + "_";
                } else {
                    textField.text = _text + " ";
                }
                cursorState = set;
            }
        }
        private void ToggleCursorState() {
            SetCursorState(!cursorState);
        }
        private void UpdateText(string text) {
            _text = text;
            textField.text = _text;
            if (typing)
                textField.text += "_";
        }

        private string ClampText(string text) {
            switch (type) {
                case TextTypes.Text16:
                    return text.Substring(0, Mathf.Min(text.Length, 16));
                case TextTypes.SixDigit: 
                    string res = text.Substring(0, Mathf.Min(text.Length, 6));
                    if (int.TryParse(res, out int _))
                        return res;
                    else return "0";
                default:
                    return text.Substring(0, Mathf.Min(text.Length, TEXT_MAX_LENGTH)); 
            }
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
            GameObject obj = instance.gameObject;
            
            Transform orig_card = orig_options.GetChild(1);
            Transform orig_bigcard = orig_card.GetChild(2);
            Transform orig_bignoise = orig_card.GetChild(6);

            GameObject bigcard = Instantiate(orig_bigcard.gameObject, obj.transform);
            bigcard.name = orig_bigcard.name;
            bigcard.SetActive(true);

            GameObject tprompt = new GameObject("Prompt");
            RectTransform menu_rect = tprompt.AddComponent<RectTransform>();
            //tprompt.AddComponent<VerticalLayoutGroup>();
            menu_rect.sizeDelta = new Vector2(514f, 299f);
            tprompt.transform.SetParent(obj.transform);
            InputField field = tprompt.AddComponent<InputField>();
            field.lineType = InputField.LineType.SingleLine;
            field.interactable = true;
            //field.onValueChanged.AddListener(instance.UpdateText);
            instance.inputField = field;

            GameObject header = new GameObject("Header");
            RectTransform hrect = header.AddComponent<RectTransform>();
            hrect.sizeDelta = new Vector2(600f, 64f);
            hrect.anchoredPosition = new Vector2(0f, 190f);
            header.transform.SetParent(tprompt.transform);
            header.AddComponent<CanvasRenderer>();
            Text htxt = APCore.CreateSettingsTextComponent(header, APCore.FontType.Bold, TextAnchor.UpperCenter, true);
            htxt.fontSize = 26;
            htxt.text = "Press [Enter] to Accept,\nPress [Esc] to Cancel. Enter Text:";

            GameObject fobj = new GameObject("Field");
            RectTransform rect = fobj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(514f, 500f);
            rect.anchoredPosition = new Vector2(0f, -15f);
            fobj.transform.SetParent(tprompt.transform);
            fobj.AddComponent<CanvasRenderer>();
            Text txt = APCore.CreateSettingsTextComponent(fobj, APCore.FontType.MonoSpace, TextAnchor.MiddleCenter, true);
            txt.fontSize = 24;
            txt.text = $"{new string('A', TEXT_MAX_LENGTH)}";
            instance.textField = txt;
            field.textComponent = txt;
            field.text = txt.text;

            GameObject bignoise = Instantiate(orig_bignoise.gameObject, obj.transform);
            bignoise.name = orig_bignoise.name;
            bignoise.SetActive(true);

            instance.initted = true;
            Logging.Log("APTypingPrompt Initialized");
        }
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Resources;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Hooks {
    internal class StartScreenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Update));
        }

        private static CupheadInput.AnyPlayerInput input;

        [HarmonyPatch(typeof(StartScreen), "Awake")]
        internal static class Awake {
            static bool Prefix() {
                if (Plugin.State < 0) {
                    Logging.LogFatal("Errors occured. Aborting game to prevent damage!");
                    CreateModErrorText();
                    input = new CupheadInput.AnyPlayerInput(false);
                    return false;
                }
                if (Config.IsTesting()) {
                    Logging.Log("Running in Test Mode: Running in test environment instead of game.");
                    CreateTestHUD();
                    CreateTestObjects();
                    input = new CupheadInput.AnyPlayerInput(false);
                    return false;
                }
                return true;
            }
            static void Postfix() {
                APClient.CloseArchipelagoSession();
            }
        }

        [HarmonyPatch(typeof(StartScreen), "Start")]
        internal static class Start {
            static bool Prefix() {
                return Plugin.State >= 0 && !Config.IsTesting();
            }
        }

        [HarmonyPatch(typeof(StartScreen), "Update")]
        internal static class Update {
            private static readonly CupheadButton[] DISMISS_BUTTONS = [
                CupheadButton.Accept, CupheadButton.Cancel, CupheadButton.Pause
            ];

            private const float TEST_DISMISS_TIME = 3;
            private static float dismissTime = 0;

            static bool Prefix() {
                if (Plugin.State < 0) {
                    if (GetAnyButtonDown(DISMISS_BUTTONS)) {
                        Application.Quit();
                    }
                    return false;
                }
                if (Config.IsTesting()) {
                    if (input?.GetButton(CupheadButton.Cancel) ?? false) {
                        if (dismissTime < TEST_DISMISS_TIME) {
                            dismissTime += Time.deltaTime;
                        }
                        else {
                            Application.Quit();
                        }
                    }
                    else {
                        dismissTime = 0;
                    }
                    return false;
                }
                return true;
            }

            private static bool GetAnyButtonDown(CupheadButton[] buttons) {
                foreach (CupheadButton button in buttons) {
                    if (input?.GetButtonDown(button) ?? false) return true;
                }
                return false;
            }
        }

        private static void CreateModErrorText() {
            GameObject canvas = new GameObject("ErrCanvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            GameObject obj = new GameObject("ModErrorText");
            obj.transform.SetParent(canvas.transform, false);
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.localPosition = new Vector3(0, 0, 0);
            rect.sizeDelta = new Vector2(400, 200);

            Text txt = obj.AddComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = FontLoader.GetFont(FontLoader.FontType.CupheadVogue_Bold_merged);
            txt.color = UnityEngine.Color.red;
            txt.fontSize = 32;
            txt.text = "CupheadArchipelago\nERROR\nCheck Log";

            obj.layer = 5;
        }

        private static void CreateTestHUD() {
            GameObject canvas = new GameObject("TestCanvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            GameObject nobj = CreateHUDText(
                "TestInfoText",
                canvas.transform,
                new(0, 0),
                new(15f, 15f),
                new(200, 100),
                "Test Mode\nHold Cancel (ESC) to Quit",
                TextAnchor.LowerLeft,
                16,
                UnityEngine.Color.white
            );
        }
        private static GameObject CreateHUDText(
            string name,
            Transform parent,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            string text,
            TextAnchor textAlignment,
            int fontSize,
            UnityEngine.Color color
        ) {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new(anchor.x, anchor.y);
            rect.anchorMax = new(anchor.x, anchor.y);
            rect.pivot = new(anchor.x, anchor.y);
            rect.anchoredPosition = new(anchoredPosition.x, anchoredPosition.y);
            rect.sizeDelta = new(sizeDelta.x, sizeDelta.y);

            Text txt = obj.AddComponent<Text>();
            txt.alignment = textAlignment;
            txt.font = FontLoader.GetFont(FontLoader.FontType.CupheadVogue_Bold_merged);
            txt.color = color;
            txt.fontSize = fontSize;
            txt.text = text;

            obj.layer = 5;

            return obj;
        }
        private static void CreateTestObjects() {
            AssetMngr.LoadBundleAssets("testee");
            GameObject mainObj = new("mainObj");

            GameObject objA = new("objA");
            objA.transform.SetParent(mainObj.transform);
            SpriteRenderer srA = objA.AddComponent<SpriteRenderer>();
            srA.sprite = AssetMngr.GetLoadedAsset<Sprite>("sqar");

            GameObject objB = new("objB");
            objB.transform.SetParent(mainObj.transform);
            SpriteRenderer srB = objB.AddComponent<SpriteRenderer>();
            srB.sprite = AssetMngr.GetLoadedAsset<Sprite>("a");

            GameObject objC = new("objC");
            objC.transform.SetParent(mainObj.transform);
            SpriteRenderer srC = objC.AddComponent<SpriteRenderer>();
            srC.sprite = AssetMngr.GetLoadedAsset<Sprite>("circ");

            objC.layer = 5;
        }
    }
}

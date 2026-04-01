/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using CupheadArchipelago.Config;
using CupheadArchipelago.TestEnv;
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
                if (MConf.IsTesting()) {
                    Logging.Log("Running in Test Mode: Running in test environment instead of game.");
                    TestMngr.Init();
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
                return Plugin.State >= 0 && !MConf.IsTesting();
            }
            static void Postfix() {
                Logging.Log($"DLC: {(DLCManager.DLCEnabled() ? "Enabled" : "Disabled")}");
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
                if (MConf.IsTesting()) {
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
    }
}

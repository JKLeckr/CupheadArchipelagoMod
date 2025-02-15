/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
                if (Plugin.State<0) {
                    Logging.LogFatal("Errors occured. Aborting game to prevent damage!");
                    CreateModErrorText();
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
                return Plugin.State>=0;
            }
        }

        [HarmonyPatch(typeof(StartScreen), "Update")]
        internal static class Update {
            static bool Prefix() {
                if (Plugin.State<0) {
                    if (input?.GetAnyButtonDown() ?? false) {
                        Application.Quit();
                    }
                    return false;
                }
                return true;
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
            rect.localPosition = new Vector3(0,0,0);
            rect.sizeDelta = new Vector2(400, 200);
            
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.Center;
            txt.OverflowMode = TextOverflowModes.Overflow;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontWeight = 800;
            txt.color = UnityEngine.Color.red;
            txt.fontSize = 32;
            txt.text = "CupheadArchipelago\nERROR\nCheck Log";
            
            obj.layer = 5;
        }
    }
}
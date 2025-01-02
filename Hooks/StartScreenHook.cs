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
        }

        [HarmonyPatch(typeof(StartScreen), "Awake")]
        internal static class Awake {
            static bool Prefix() {
                return Plugin.State>=0;
            }
            static void Postfix() {
                APClient.CloseArchipelagoSession();
            }
        }

        [HarmonyPatch(typeof(StartScreen), "Start")]
        internal static class Start {
            static bool Prefix() {
                if (Plugin.State<0) {
                    Logging.LogFatal("Errors occured. Aborting to prevent damage!");
                    CreateModErrorText();
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
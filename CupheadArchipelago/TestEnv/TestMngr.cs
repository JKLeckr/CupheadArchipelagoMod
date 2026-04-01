/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.TestEnv {
    internal class TestMngr {
        public static void Init() {
            CreateTestHUD();
            CreateTestObjects();
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

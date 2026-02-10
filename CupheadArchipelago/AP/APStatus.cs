/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using UnityEngine;
using TMPro;

namespace CupheadArchipelago.AP {
    public class APStatus {
        private GameObject _instance = null;
        private TextMeshProUGUI _txt = null;

        public static APStatus CreateAPStatus() {
            APStatus res = new APStatus();
            res.Create();
            return res;
        }
        private void Create() {
            GameObject p = new GameObject("APStatus");
            Object.DontDestroyOnLoad(p);
            p.transform.SetPosition(0,25,0);
            GameObject c = new GameObject("Canvas");
            c.layer = 5;
            c.transform.SetParent(p.transform);
            /*GameObject cam = new GameObject("UICamera(AP)");
            cam.transform.SetParent(p.transform);
            Camera camc = cam.AddComponent<Camera>();
            camc.fieldOfView = 30;
            camc.nearClipPlane = 0.01f;
            camc.farClipPlane = 10000;
            camc.allowHDR = false;
            camc.orthographicSize = 360;
            camc.orthographic = true;
            camc.depth = 10;
            camc.cullingMask = 32;
            camc.backgroundColor = Color.black;
            cam.AddComponent<CupheadUICamera>();*/
            Canvas canv = c.AddComponent<Canvas>();
            /*canv.renderMode = RenderMode.ScreenSpaceCamera;
            canv.worldCamera = camc;*/
            GameObject tobj = new GameObject("APStatusText");
            tobj.transform.SetParent(c.transform);

            RectTransform rect = tobj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f,1);
            rect.anchorMax = new Vector2(0.5f,1);
            rect.offsetMin = new Vector2(-375,1000);
            rect.offsetMax = new Vector2(-295,-15);
            rect.pivot = new Vector2(0.5f,1);
            rect.position = new Vector3(5,345,100);

            TextMeshProUGUI txt = tobj.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.OverflowMode = TextOverflowModes.Overflow;
            txt.color = new Color(.4f,.4f,.4f);
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontWeight = 900;
            txt.fontSize = 32;
            txt.text = "TESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTEST";

            tobj.layer = 5;

            _instance = p;
            _txt = txt;
        }

        public void SetText(string str) {
            _txt.SetText(str);
        }

        private void Destroy() {
            Object.Destroy(_instance);
            _instance = null;
        }
        public static void DestroyAPStatus(ref APStatus obj) {
            obj.Destroy();
            obj = null;
        }
        ~APStatus() {
            if (_instance!=null) Destroy();
        }
    }
}

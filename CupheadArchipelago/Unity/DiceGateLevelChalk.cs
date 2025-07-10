/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.AP;
using CupheadArchipelago.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Unity {
    internal class DiceGateLevelChalk : MonoBehaviour {
        private bool _initted = false;

        [SerializeField]
        private bool shaderControls = false;
        [SerializeField]
        private bool canvasControls = true;

        private SpriteRenderer bgsr = null;
        private MaterialPropertyBlock bgmblock = null;
        private Vector2 bgopos = new(0.22384f, 0.0743f);
        private float bgosize = 0.031175f;
        private float bgotrim = 0.96f;
        private float cmult = 1;

        RectTransform trect = null;
        RectTransform t2rect = null;
        Text chalkTxt = null;
        Text chalk2Txt = null;

        public void Init(int contracts, int requiredContracts) {
            if (_initted) return;
            CreateChalkOverlay();
            chalkTxt.text = $"{contracts}";
            chalk2Txt.text = $"{requiredContracts}";
            _initted = true;
        }

        void FixedUpdate() {
            if (_initted) {
                if (shaderControls || canvasControls) ControlUpdate();
                if (shaderControls) ShaderUpdate();
                if (canvasControls) CanvasUpdate();
            }
        }

        // TODO: Improve the Shader
        private void CreateChalkOverlay() {
            Transform cam = transform.GetChild(0);
            Transform bgparent = transform.GetChild(3);
            Transform obj = bgparent.GetChild(3);
            Transform slashTemplate = bgparent.GetChild(7);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            Texture2D oTex = AssetMngr.GetLoadedAsset<Texture2D>("cap_dicehouse_chalkboard_tics");
            sr.material.shader = StaticAssets.OverlayShader;
            MaterialPropertyBlock mblock = new();
            sr.GetPropertyBlock(mblock);

            mblock.SetTexture("_OverlayTex", oTex);
            mblock.SetVector("_OverlayPos", new(bgopos.x, bgopos.y, 0f, 0f));
            mblock.SetVector("_OverlaySize", new(bgosize, bgosize, 0f, 0f));
            mblock.SetFloat("_TrimAlpha", bgotrim);

            sr.SetPropertyBlock(mblock);

            //bgsr = sr;
            //bgmblock = mblock;

            GameObject slashObj = Instantiate(slashTemplate.gameObject, bgparent);
            slashObj.name = "die_house_chalk_slash";
            slashObj.SetActive(true);
            slashObj.transform.SetSiblingIndex(4);
            slashObj.transform.position = new Vector3(197.5f, 22f, 0f);
            slashObj.transform.SetScale(2.80f, 1.50f, 1f);
            slashObj.transform.SetEulerAngles(20f, 20f, 38f);

            GameObject toObj = new("die_house_chalk");
            RectTransform crect = toObj.AddComponent<RectTransform>();
            crect.SetParent(bgparent);
            crect.SetSiblingIndex(4);
            Canvas canvas = toObj.AddComponent<Canvas>();
            toObj.AddComponent<CanvasScaler>();
            toObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam.GetComponent<Camera>();
            canvas.sortingOrder = 5;
            crect.position = Vector3.zero;
            crect.SetScale(1f, 1f, 1f);

            GameObject txtObj = new("ChalkText");
            trect = txtObj.AddComponent<RectTransform>();
            trect.SetParent(crect);
            trect.transform.SetPosition(96f, -1f, 0f);
            trect.transform.SetEulerAngles(20f, 20f, 0f);
            txtObj.AddComponent<CanvasRenderer>();
            chalkTxt = txtObj.AddComponent<Text>();
            chalkTxt.color = new(1f, 1f, 1f, 0.72f);
            chalkTxt.font = FontLoader.GetFont(FontLoader.FontType.CupheadVogue_Bold_merged);
            chalkTxt.fontSize = 32;
            chalkTxt.alignment = TextAnchor.LowerRight;
            chalkTxt.text = "0";

            GameObject txt2Obj = Instantiate(txtObj, crect);
            t2rect = txt2Obj.GetComponent<RectTransform>();
            t2rect.transform.SetPosition(204f, -90f, 0f);
            t2rect.transform.SetEulerAngles(20f, 20f, 0f);
            txt2Obj.name = "ChalkText2";
            chalk2Txt = txt2Obj.GetComponent<Text>();
            chalk2Txt.alignment = TextAnchor.UpperLeft;
            chalk2Txt.text = "0";
        }

        private static int ClampReqIndex(int i) {
            if (i <= 0) return 0;
            else if (i >= APSettings.RequiredContracts.Length) return APSettings.RequiredContracts.Length - 1;
            else return i;
        }

        private void ControlUpdate() {
            bool update = false;
            if (Input.GetKeyDown(KeyCode.N)) {
                update = true;
                cmult -= 0.025f;
                if (cmult < 0) cmult = 0;
            }
            if (Input.GetKeyDown(KeyCode.M)) {
                update = true;
                cmult += 0.025f;
                if (cmult > 2) cmult = 2;
            }
            if (Input.GetKeyDown(KeyCode.Comma)) {
                update = true;
                cmult -= 0.0025f;
                if (cmult < 0) cmult = 0;
            }
            if (Input.GetKeyDown(KeyCode.Period)) {
                update = true;
                cmult += 0.0025f;
                if (cmult > 255) cmult = 2;
            }
            if (update) {
                Logging.Log($"mult: {cmult}");
            }
        }

        private void ShaderUpdate() {
            bool update = false;
            float mult = cmult / 100;
            if (Input.GetKey(KeyCode.T)) {
                update = true;
                bgopos.y += mult;
            }
            if (Input.GetKey(KeyCode.G)) {
                update = true;
                bgopos.y -= mult;
            }
            if (Input.GetKey(KeyCode.F)) {
                update = true;
                bgopos.x -= mult;
            }
            if (Input.GetKey(KeyCode.H)) {
                update = true;
                bgopos.x += mult;
            }
            if (Input.GetKey(KeyCode.I)) {
                update = true;
                bgosize += mult;
            }
            if (Input.GetKey(KeyCode.K)) {
                update = true;
                bgosize -= mult;
            }
            if (Input.GetKey(KeyCode.O)) {
                update = true;
                bgotrim += mult / 10;
            }
            if (Input.GetKey(KeyCode.L)) {
                update = true;
                bgotrim -= mult / 10;
            }
            if (Input.GetKey(KeyCode.V)) {
                update = true;
                bgsr.GetPropertyBlock(bgmblock);
                Texture2D oTex = AssetMngr.GetLoadedAsset<Texture2D>("cap_dicehouse_chalkboard_tics");
                bgmblock.SetTexture("_OverlayTex", oTex);
                bgsr.SetPropertyBlock(bgmblock);
            }
            else if (Input.GetKey(KeyCode.B)) {
                update = true;
                bgsr.GetPropertyBlock(bgmblock);
                bgmblock.SetTexture("_OverlayTex", Texture2D.whiteTexture);
                bgsr.SetPropertyBlock(bgmblock);
            }
            else if (bgsr != null && bgmblock != null && update) {
                bgsr.GetPropertyBlock(bgmblock);
                bgmblock.SetVector("_OverlayPos", new(bgopos.x, bgopos.y, 0f, 0f));
                bgmblock.SetVector("_OverlaySize", new(bgosize, bgosize, 0f, 0f));
                bgmblock.SetFloat("_TrimAlpha", bgotrim);
                bgsr.SetPropertyBlock(bgmblock);
            }
            if (update) {
                Logging.Log($"({bgopos.x},{bgopos.y}) ({bgosize}) ({cmult}) ({bgotrim})");
            }
        }

        private void CanvasUpdate() {
            bool update = false;
            float mult = cmult;
            if (Input.GetKey(KeyCode.T)) {
                update = true;
                trect.AddPosition(0, mult);
            }
            if (Input.GetKey(KeyCode.G)) {
                update = true;
                trect.AddPosition(0, -mult);
            }
            if (Input.GetKey(KeyCode.F)) {
                update = true;
                trect.AddPosition(-mult, 0);
            }
            if (Input.GetKey(KeyCode.H)) {
                update = true;
                trect.AddPosition(mult, 0);
            }
            if (Input.GetKey(KeyCode.I)) {
                update = true;
                t2rect.AddPosition(0, mult);
            }
            if (Input.GetKey(KeyCode.K)) {
                update = true;
                t2rect.AddPosition(0, -mult);
            }
            if (Input.GetKey(KeyCode.J)) {
                update = true;
                t2rect.AddPosition(-mult, 0);
            }
            if (Input.GetKey(KeyCode.L)) {
                update = true;
                t2rect.AddPosition(mult, 0);
            }
            if (Input.GetKey(KeyCode.V)) {
                update = true;
                //crect.AddPosition(0, 0, -mult);
            }
            else if (Input.GetKey(KeyCode.B)) {
                update = true;
                //crect.AddPosition(0, 0, mult);
            }
            if (update) {
                Vector3 pos = trect.position;
                Vector3 pos2 = t2rect.position;
                Logging.Log($"({pos.x},{pos.y}) ({pos2.x},{pos2.y}) ({cmult})");
            }
        }
    }
}

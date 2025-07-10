/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Resources;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RektTransform;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DiceGateLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(DiceGateLevel), "Start")]
        internal static class Start {
            static bool Prefix(DiceGateLevel __instance) {
                if (APData.IsCurrentSlotEnabled()) {}
                CreateChalkOverlay(__instance.transform);
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int index = 1;
                int req_index = 0;
                int insertCount = 0;

                MethodInfo _mi_get_PlayerData_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelCompleted = typeof(PlayerData).GetMethod("CheckLevelCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_CheckLevelsCompleted = typeof(PlayerData).GetMethod("CheckLevelsCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APTallyCondition = typeof(Start).GetMethod("APTallyCondition", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APOpenCondition = typeof(Start).GetMethod("APOpenCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_PlayerData_Data &&
                        codes[i + 2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i + 2].operand == _mi_CheckLevelCompleted ||
                        (MethodInfo)codes[i + 2].operand == _mi_CheckLevelsCompleted) && codes[i + 3].opcode == OpCodes.Brfalse) {
                        bool countCondition = codes[i + 1].opcode == OpCodes.Ldsfld;
                        int dieIndex = ClampReqIndex(req_index);
                        int testCount = index;
                        CodeInstruction[] ncodes = countCondition ? [
                            new CodeInstruction(OpCodes.Ldc_I4, dieIndex),
                                new CodeInstruction(OpCodes.Call, _mi_APOpenCondition),
                            ] : [
                            new CodeInstruction(OpCodes.Ldc_I4, dieIndex),
                                new CodeInstruction(OpCodes.Ldc_I4, testCount),
                                new CodeInstruction(OpCodes.Call, _mi_APTallyCondition),
                            ];
                        codes.InsertRange(i + 3, ncodes);
                        i += ncodes.Length;
                        if (countCondition) req_index++;
                        else index++;
                        insertCount++;
                    }
                }
                if (insertCount != 12) throw new Exception($"{nameof(Start)}: Patch Failed! insertCount: {insertCount}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            /*static void Postfix(DiceGateLevel __instance) {
                __instance.StartCoroutine(FixedUpdate_cr());
            }*/

            private static IEnumerator FixedUpdate_cr() {
                while (true) {
                    //ShaderUpdate();
                    yield return new WaitForFixedUpdate();
                }
            }

            private static int ClampReqIndex(int i) {
                if (i <= 0) return 0;
                else if (i >= APSettings.RequiredContracts.Length) return APSettings.RequiredContracts.Length - 1;
                else return i;
            }
            private static bool APTallyCondition(bool vanillaCondition, int testIndex, int testCount) {
                return !APData.IsCurrentSlotEnabled() && vanillaCondition;
                /*if (APData.IsCurrentSlotEnabled()) {
                    int contractReq = APSettings.RequiredContracts[testIndex];
                    bool maxCount = testCount > contractReq;
                    if (!maxCount) Logging.LogDebug($"TallyContracts: {APClient.APSessionGSPlayerData.contracts}>={testCount}");
                    return APClient.APSessionGSPlayerData.contracts >= testCount || maxCount;
                }
                else return vanillaCondition;*/
            }
            private static bool APOpenCondition(bool vanillaCondition, int testIndex) {
                if (APData.IsCurrentSlotEnabled()) {
                    int testCount = APSettings.RequiredContracts[testIndex];
                    Logging.Log($"ReqContracts: {APClient.APSessionGSPlayerData.contracts}>={testCount}");
                    return APClient.APSessionGSPlayerData.contracts >= testCount;
                }
                else return vanillaCondition;
            }
        }

        /*private static SpriteRenderer bgsr = null;
        private static MaterialPropertyBlock bgmblock = null;
        private static Vector2 bgopos = new(0.22384f, 0.0743f);
        private static float bgosize = 0.031175f;
        private static float bgotrim = 0.9f;
        private static float bgmult = 1;

        private static void ShaderUpdate() {
            bool update = false;
            float mult = bgmult / 100;
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
            if (Input.GetKeyDown(KeyCode.N)) {
                update = true;
                bgmult -= 0.025f;
                if (bgmult < 0) bgmult = 0;
            }
            if (Input.GetKeyDown(KeyCode.M)) {
                update = true;
                bgmult += 0.025f;
                if (bgmult > 2) bgmult = 2;
            }
            if (Input.GetKeyDown(KeyCode.Comma)) {
                update = true;
                bgmult -= 0.0025f;
                if (bgmult < 0) bgmult = 0;
            }
            if (Input.GetKeyDown(KeyCode.Period)) {
                update = true;
                bgmult += 0.0025f;
                if (bgmult > 2) bgmult = 2;
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
                Logging.Log($"({bgopos.x},{bgopos.y}) ({bgosize}) ({bgmult}) ({bgotrim})");
            }
        }*/

        // TODO: Improve the Shader
        private static void CreateChalkOverlay(Transform parent) {
            Transform cam = parent.GetChild(0);
            Transform obj = parent.GetChild(3).GetChild(3);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            Texture2D oTex = AssetMngr.GetLoadedAsset<Texture2D>("cap_dicehouse_chalkboard_tics");
            sr.material.shader = StaticAssets.OverlayShader;
            MaterialPropertyBlock mblock = new();
            sr.GetPropertyBlock(mblock);

            mblock.SetTexture("_OverlayTex", oTex);
            mblock.SetVector("_OverlayPos", new(0.22384f, 0.0743f, 0f, 0f));
            mblock.SetVector("_OverlaySize", new(0.031175f, 0.031175f, 0f, 0f));
            mblock.SetFloat("_TrimAlpha", 0.96f);

            sr.SetPropertyBlock(mblock);

            //bgsr = sr;
            //bgmblock = mblock;

            GameObject toObj = new("ChalkTextCanvas");
            RectTransform rect = toObj.AddComponent<RectTransform>();
            rect.SetParent(obj.transform);
            Canvas canvas = toObj.AddComponent<Canvas>();
            toObj.AddComponent<CanvasScaler>();
            toObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam.GetComponent<Camera>();
            rect.position = Vector3.zero;
            rect.SetScale(0.015f, 0.015f);
            rect.SetWidth(1040f);
            rect.SetHeight(560f);

            GameObject txtObj = new("ChalkText");
            RectTransform trect = txtObj.AddComponent<RectTransform>();
            trect.SetParent(rect);
            txtObj.AddComponent<CanvasRenderer>();
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.color = Color.white;
            txt.font = FontLoader.GetTMPFont(FontLoader.TMPFontType.CupheadVogue_Bold_merged__SDF);
            txt.fontWeight = 600;
            txt.fontSize = 24;
            txt.alignment = TextAlignmentOptions.Center;
            txt.text = "00 / 00";

            txtObj.layer = 5;
        }
    }
}

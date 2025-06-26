/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Resources;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DiceGateLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(DiceGateLevel), "Start")]
        internal static class Start {
            static bool Prefix(DiceGateLevel __instance) {
                try {
                    CreateChalkOverlay(__instance.transform);
                }
                catch (Exception e) {
                    Logging.LogError($"Could not load chalkboard sprite {e}");
                }
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

            private static int ClampReqIndex(int i) {
                if (i <= 0) return 0;
                else if (i >= APSettings.RequiredContracts.Length) return APSettings.RequiredContracts.Length - 1;
                else return i;
            }
            private static bool APTallyCondition(bool vanillaCondition, int testIndex, int testCount) {
                if (APData.IsCurrentSlotEnabled()) {
                    int contractReq = APSettings.RequiredContracts[testIndex];
                    bool maxCount = testCount > contractReq;
                    if (!maxCount) Logging.LogDebug($"TallyContracts: {APClient.APSessionGSPlayerData.contracts}>={testCount}");
                    return APClient.APSessionGSPlayerData.contracts >= testCount || maxCount;
                }
                else return vanillaCondition;
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

        private static void CreateChalkOverlay(Transform parent) {
            Transform _parent = parent.GetChild(3);
            GameObject obj = GameObject.Instantiate(_parent.GetChild(4).gameObject);
            obj.transform.SetParent(_parent);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sprite = AssetMngr.GetLoadedAsset<Sprite>("cap_dicehouse_chalkboard_spr");
            /*GameObject obj = GameObject.Instantiate(AssetMngr.GetLoadedAsset<GameObject>("cap_dicehouse_chalkboard"), _parent);
            Logging.Log("1");
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            Logging.Log("1");
            Material mat_default = new(Shader.Find("Sprites/Default"));
            Logging.Log("1");
            sr.material = mat_default;
            Logging.Log("1");
            Transform quad = obj.transform.GetChild(0);
            Logging.Log("1");
            MeshRenderer qsr = quad.GetComponent<MeshRenderer>();
            Logging.Log("1");
            qsr.material = mat_default;
            qsr.materials = [mat_default];
            Logging.Log("1");*/
            /*Texture2D tex = AssetMngr.GetLoadedAsset<Texture2D>("cap_dicehouse_chalkboard_spr");
            Sprite sprChalk = Sprite.Create(tex, new(0f, 0f, tex.width, tex.height), new(0.5f, 0.5f), 100f);
            GameObject chalkOverlay = new("die_house_chalkoverlay");
            chalkOverlay.transform.parent = parent;
            chalkOverlay.transform.position = new Vector2(0, 0);
            SpriteRenderer sr = chalkOverlay.AddComponent<SpriteRenderer>();
            sr.sprite = sprChalk;*/

        }
    }
}

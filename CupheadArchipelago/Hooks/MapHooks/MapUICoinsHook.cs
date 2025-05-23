/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapUICoinsHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(Update));
        }

        [HarmonyPatch(typeof(MapUICoins), "Start")]
        internal static class Start {
            static void Postfix(MapUICoins __instance, ref PlayerId ___playerId, Image ___coinImage) {
                if (!APData.IsCurrentSlotEnabled() || !APSettings.UseDLC) return;
                if (__instance.name == "CurrencyCanvasTwo") {
                    /*GameObject currencyOne = GameObject.Find("CurrencyCanvasOne");
                    Vector2 diff = __instance.GetComponent<RectTransform>().anchoredPosition - currencyOne.GetComponent<RectTransform>().anchoredPosition;
                    Logging.Log($"HUD Diff: {diff.x}, {diff.y}");*/
                    GameObject currencyThree = UnityEngine.Object.Instantiate(__instance.gameObject, __instance.transform.parent);
                    currencyThree.name = "CurrencyCanvasThree";
                    currencyThree.transform.SetSiblingIndex(2);
                    RectTransform rect = currencyThree.GetComponent<RectTransform>();
                    rect.anchoredPosition += new Vector2(108.2598f, 0f);
                    GameObject icon = SetupHUDIcon(__instance.gameObject, "C");
                    RectTransform trect = icon.gameObject.GetComponent<RectTransform>();
                    trect.anchoredPosition += new Vector2(24f, -20f);
                    Logging.Log("Initialized Ingredients HUD");
                }
                else if (__instance.name == "CurrencyCanvasThree") {
                    ___playerId = PlayerId.Any;
                    ___coinImage.enabled = false;
                    GameObject icon = SetupHUDIcon(__instance.gameObject, "I");
                    RectTransform trect = icon.gameObject.GetComponent<RectTransform>();
                    trect.anchoredPosition += new Vector2(36f, -20f);
                }
            }

            private static GameObject SetupHUDIcon(GameObject obj, string str) {
                Transform tobj = obj.transform.GetChild(2);
                tobj.gameObject.SetActive(true);
                TextMeshProUGUI text = tobj.GetComponent<TextMeshProUGUI>();
                text.text = str;
                text.fontSize -= 16;
                text.color = new Color(.97f, .85f, .28f);
                return tobj.gameObject;
            }
        }

        [HarmonyPatch(typeof(MapUICoins), "Update")]
        internal static class Update {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_Multiplayer = typeof(PlayerManager).GetField("Multiplayer", BindingFlags.Public | BindingFlags.Static);
                FieldInfo _fi_playerId = typeof(MapUICoins).GetField("playerId", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_GetCurrency = typeof(PlayerData).GetMethod("GetCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APMult = typeof(Update).GetMethod("APMult", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APGetCurrency = typeof(Update).GetMethod("APGetCurrency", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if ((success&1)==0 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == _fi_Multiplayer && codes[i+1].opcode == OpCodes.Brfalse) {
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_APMult));
                        i++;
                        success |= 1;
                    }
                    if ((success&2)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldarg_0 &&
                        codes[i+2].opcode == OpCodes.Ldfld && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_GetCurrency &&
                        codes[i+4].opcode == OpCodes.Stloc_0) {
                            codes[i+3] = new CodeInstruction(OpCodes.Call, _mi_APGetCurrency);
                            codes[i+1].labels = codes[i].labels;
                            codes.RemoveAt(i);
                            success |= 2;
                    }
                    if (success==3) break;
                }
                if (success!=3) throw new Exception($"{nameof(Update)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            static void Postfix(PlayerId ___playerId, Image ___coinImage) {
                if (APData.IsCurrentSlotEnabled() && ___playerId == PlayerId.PlayerTwo) ___coinImage.enabled = false;
            }

            private static bool APMult(bool multiplayer) => multiplayer || APData.IsCurrentSlotEnabled();
            private static int APGetCurrency(PlayerId playerId) {
                if (APData.IsCurrentSlotEnabled() && playerId != PlayerId.PlayerOne) {
                    if (playerId == PlayerId.PlayerTwo)
                        return APData.CurrentSData.playerData.contracts;
                    if (playerId == PlayerId.Any)
                        return APData.CurrentSData.playerData.dlc_ingredients;
                }
                return PlayerData.Data.GetCurrency(playerId);
            }
        }
    }
}

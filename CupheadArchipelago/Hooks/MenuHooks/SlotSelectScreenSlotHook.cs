/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace CupheadArchipelago.Hooks.MenuHooks {
    internal class SlotSelectScreenSlotHook {
        private static SlotSelectScreenSlot[] instances;
        private static GameObject[] slotAPText = null;
        private static byte[] states;
        private const int APTEXT_SIBLING_INDEX = 3;
        private static readonly Color hColor = new Color(.82f,.76f,.70f);
        private static readonly Color bColor = new Color(.18f,.18f,.18f);
        private static readonly Color ehColor = new Color(.32f,.18f,.18f);
        private static readonly Color ebColor = new Color(.82f,.18f,.18f);

        internal static void Hook() {
            instances = new SlotSelectScreenSlot[3];
            slotAPText = new GameObject[instances.Length];
            states = new byte[instances.Length];
            Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(SetSelected));
        }

        [HarmonyPatch(typeof(SlotSelectScreenSlot), "Init")]
        internal static class Init {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;
                bool labelPlaced = false;

                FieldInfo _fi_MapData_sessionStarted = typeof(PlayerData.MapData).GetField("sessionStarted", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_emptyChild = typeof(SlotSelectScreenSlot).GetField("emptyChild", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_gameObject_SetActive = typeof(GameObject).GetMethod("SetActive");
                MethodInfo _mi_IsAPEmpty = typeof(Init).GetMethod("IsAPEmpty", BindingFlags.NonPublic | BindingFlags.Static);

                Label tgt_label = il.DefineLabel();

                //Logging.Log(_fi_MapData_sessionStarted);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if (!success && codes[i].opcode == OpCodes.Ldloc_0 && codes[i+1].opcode == OpCodes.Ldc_I4_3 && 
                        codes[i+2].opcode == OpCodes.Callvirt && codes[i+3].opcode == OpCodes.Ldfld && 
                        (FieldInfo)codes[i+3].operand == _fi_MapData_sessionStarted && codes[i+4].opcode == OpCodes.Brtrue) {
                            CodeInstruction[] ncodes = [
                                new CodeInstruction(OpCodes.Ldarg_1),
                                new CodeInstruction(OpCodes.Call, _mi_IsAPEmpty),
                                new CodeInstruction(OpCodes.Brtrue, tgt_label),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Length;
                            success = true;
                    }
                    if (!labelPlaced && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && 
                        (FieldInfo)codes[i+1].operand == _fi_emptyChild && codes[i+2].opcode == OpCodes.Callvirt && 
                        codes[i+3].opcode == OpCodes.Ldc_I4_1 && codes[i+4].opcode == OpCodes.Callvirt && 
                        (MethodInfo)codes[i+4].operand == _mi_gameObject_SetActive) {
                            codes[i].labels.Add(tgt_label);
                            labelPlaced = true;
                    }
                    if (success&&labelPlaced) break;
                }
                if (!labelPlaced||!success) throw new Exception($"{nameof(Init)}: Patch Failed! {labelPlaced}||{success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            static void Postfix(
                SlotSelectScreenSlot __instance,
                int slotNumber//, //TODO: Add text percent for AP Progress
                /*TMP_Text ___slotTitle,
                TMP_Text ___slotPercentage,
                TMP_Text ___slotPercentageSelectedBase,
                TMP_Text ___slotPercentageSelectedDLC*/
                ) {
                //Logging.Log.LogInfo("Init");
                //Logging.Log.LogInfo(__instance.gameObject.name);
                instances[slotNumber] = __instance;
                states[slotNumber] = 0;

                if (slotAPText[slotNumber] != null) {
                    UnityEngine.Object.Destroy(slotAPText[slotNumber]);
                    slotAPText[slotNumber] = null;
                }

                CreateSlotAPText(slotNumber, __instance.transform);

                /*if (APData.IsSlotEnabled(slotNumber) && !APData.IsSlotEmpty(slotNumber)) {
                    // Add this later
                }*/
            }

            private static bool IsAPEmpty(int slot) => APData.IsSlotEnabled(slot) && APData.IsSlotEmpty(slot);
        }
        [HarmonyPatch(typeof(SlotSelectScreenSlot), "SetSelected")]
        internal static class SetSelected {
            static void Postfix(SlotSelectScreenSlot __instance, ref bool selected, RectTransform ___mainChild, RectTransform ___mainDLCChild) {
                //Logging.Log.LogInfo("SetSelected");
                //Logging.Log.LogInfo(__instance.gameObject.name);
                int slot = GetSlotNumber(__instance);
                //Transform slotAPText_inst = __instance.transform.GetChild(__instance.transform.childCount-1);
                Transform slotAPText_inst = __instance.transform.GetChild(APTEXT_SIBLING_INDEX);
                if (slotAPText_inst!=null) {
                    TextMeshProUGUI txt = slotAPText_inst.GetComponent<TextMeshProUGUI>();
                    if (APData.IsSlotEnabled(slot) && APData.SData[slot].state==0) {
                        txt.color = selected?hColor:bColor;
                        slotAPText_inst.gameObject.SetActive(true);
                        states[slot] = 1;
                    }
                    else if (APData.SData[slot].state<0) {
                        txt.color = selected?ehColor:ebColor;
                        txt.text = "E" + APData.SData[slot].state;
                        slotAPText_inst.gameObject.SetActive(true);
                        states[slot] = 2;                            
                    }
                    else if (APData.SData[slot].state>0) {
                        txt.color = selected?ehColor:ebColor;
                        txt.text = "!" + APData.SData[slot].state;
                        slotAPText_inst.gameObject.SetActive(true);
                        states[slot] = 3;                            
                    }
                    else {
                        slotAPText_inst.gameObject.SetActive(false);
                        states[slot] = 0;
                    }
                }
                /*if (APData.IsSlotEnabled(slot) && !APData.IsSlotEmpty(slot)) {
                    ___mainChild.gameObject.SetActive(!selected);
                    ___mainDLCChild.gameObject.SetActive(selected);
                }*/
            }
        }

        private static int GetSlotNumber(SlotSelectScreenSlot instance) {
            int i = 0;
            foreach (SlotSelectScreenSlot slot in instances) {
                if (instance == slot) return i;
                else i++;
            }
            return -1;
        }
        private static void CreateSlotAPText(int index, Transform parent=null) {
            GameObject obj = new GameObject("SlotAPText");
            obj.SetActive(false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.96f,0.5f);
            rect.AddLocalPosition(-1.85f,5f,0f);
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.color = Color.black;
            txt.fontSize = 32;
            txt.text = "AP";
            obj.layer = 5;

            if (parent!=null) {
                rect.SetParent(parent,false);
                rect.SetSiblingIndex(APTEXT_SIBLING_INDEX);
            }
            else {
                Debug.LogError("[CreateSlotAPText] Parent is null!");
            }

            slotAPText[index] = obj;    
        }
    }
}

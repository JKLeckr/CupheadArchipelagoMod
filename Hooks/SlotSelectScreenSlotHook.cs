/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace CupheadArchipelago.Hooks {
    public class SlotSelectScreenSlotHook {
        private static SlotSelectScreenSlot[] instances;
        private static GameObject[] slotAPText = null;
        private const int APTEXT_SIBLING_INDEX = 3;
        private static readonly Color hColor = new Color(.82f,.76f,.70f);
        private static readonly Color bColor = new Color(.18f,.18f,.18f);

        public static void Hook() {
            instances = new SlotSelectScreenSlot[3];
            slotAPText = new GameObject[instances.Length];
            Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(SetSelected));
        }

        [HarmonyPatch(typeof(SlotSelectScreenSlot), "Init")]
        internal static class Init {
            static void Postfix(SlotSelectScreenSlot __instance, ref int slotNumber) {
                //Plugin.Log.LogInfo("Init");
                //Plugin.Log.LogInfo(__instance.gameObject.name);
                instances[slotNumber] = __instance;

                if (slotAPText[slotNumber] != null) {
                    Object.Destroy(slotAPText[slotNumber]);
                    slotAPText[slotNumber] = null;
                }

                CreateSlotAPText(slotNumber, __instance.transform);
            }
        }
        [HarmonyPatch(typeof(SlotSelectScreenSlot), "SetSelected")]
        internal static class SetSelected {
            static void Postfix(SlotSelectScreenSlot __instance, ref bool selected) {
                //Plugin.Log.LogInfo("SetSelected");
                //Plugin.Log.LogInfo(__instance.gameObject.name);
                int slot = GetSlotNumber(__instance);
                //Transform slotAPText_inst = __instance.transform.GetChild(__instance.transform.childCount-1);
                Transform slotAPText_inst = __instance.transform.GetChild(APTEXT_SIBLING_INDEX);
                if (slotAPText_inst!=null) {
                    TextMeshProUGUI txt = slotAPText_inst.GetComponent<TextMeshProUGUI>();
                    if (APData.SData[slot].enabled) {
                        txt.color = selected?hColor:bColor;
                        slotAPText_inst.gameObject.SetActive(true);
                    }
                    else {
                        slotAPText_inst.gameObject.SetActive(false);
                    }
                }
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
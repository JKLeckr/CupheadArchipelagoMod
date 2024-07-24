/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Util;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopScenePlayerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Start));
            //Harmony.CreateAndPatchAll(typeof(UpdateSelection));
            Harmony.CreateAndPatchAll(typeof(addNewItem_cr));
        }

        /* SHOP LAYOUT: WCWCW */
        // Order: BW, BC, DW, DC
        private static readonly int[][] shopOrders = [
            [0,1,2,3,4,5,6,7],
            [0,1,2,3,4,5,6,7],
            [7,6,5,4,3,0,1,2],
            [7,6,5,4,3,2,0,1]
        ];

        private static readonly Dictionary<Scenes, int> worldIndexes = new() {
            {Scenes.scene_map_world_1, 0},
            {Scenes.scene_map_world_2, 1},
            {Scenes.scene_map_world_3, 2},
            {Scenes.scene_map_world_DLC, 3},
        };
        private static readonly Dictionary<Weapon, int> weaponOrder = new() {
            {Weapon.level_weapon_homing, 0},
            {Weapon.level_weapon_spreadshot, 1},
            {Weapon.level_weapon_boomerang, 2},
            {Weapon.level_weapon_bouncer, 3},
            {Weapon.level_weapon_charge, 4},
            {Weapon.level_weapon_wide_shot, 5},
            {Weapon.level_weapon_crackshot, 6},
            {Weapon.level_weapon_upshot, 7},
        };
        private static readonly Dictionary<Charm, int> charmOrder = new() {
            {Charm.charm_health_up_1, 0},
            {Charm.charm_smoke_dash, 1},
            {Charm.charm_parry_plus, 2},
            {Charm.charm_super_builder, 3},
            {Charm.charm_parry_attack, 4},
            {Charm.charm_health_up_2, 5},
            {Charm.charm_healer, 6},
            {Charm.charm_curse, 7},
        };

        //private static Dictionary<Weapon, APLocation> WeaponLocations { get => ShopSceneItemHook.weaponLocations; }
        //private static Dictionary<Charm, APLocation> CharmLocations { get => ShopSceneItemHook.charmLocations; }

        [HarmonyPatch(typeof(ShopScenePlayer), "Awake")]
        internal static class Awake {
            static bool Prefix(List<ShopSceneItem> ___items, ShopSceneItem[] ___charmItemPrefabs, ShopSceneItem[] ___weaponItemPrefabs) {
                Plugin.Log("---SHOP---");
                Plugin.Log("-items-");
                foreach (ShopSceneItem item in ___items) 
                    Plugin.Log(item.ToString());
                Plugin.Log("\n-charmItemPrefabs-");
                foreach (ShopSceneItem item in ___charmItemPrefabs) 
                    Plugin.Log(item.ToString());
                Plugin.Log("\n-weaponItemPrefabs-");
                foreach (ShopSceneItem item in ___weaponItemPrefabs) 
                    Plugin.Log(item.ToString());
                Plugin.Log("\n---END SHOP---");
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                byte successBit = 0;
                FieldInfo _fi_Multiplayer = typeof(PlayerManager).GetField("Multiplayer", BindingFlags.Public | BindingFlags.Static);
                FieldInfo _fi_items = typeof(ShopScenePlayer).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weaponItemPrefabs = typeof(ShopScenePlayer).GetField("weaponItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_charmItemPrefabs = typeof(ShopScenePlayer).GetField("charmItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weaponIndex = typeof(ShopScenePlayer).GetField("weaponIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_gameObject_SetActive = typeof(GameObject).GetMethod("SetActive");
                MethodInfo _mi_SetupItems = typeof(Awake).GetMethod(nameof(SetupItems), BindingFlags.NonPublic | BindingFlags.Static);

                Label tgt_label = il.DefineLabel();

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-6;i++) {
                    if ((successBit&1) == 0 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == _fi_Multiplayer && codes[i+1].opcode == OpCodes.Brtrue) {
                        List<CodeInstruction> ncodes = [
                            CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                            new CodeInstruction(OpCodes.Brtrue, tgt_label),
                        ];
                        codes.InsertRange(i, ncodes);
                        i+=ncodes.Count;
                        successBit|=1;
                    }
                    if ((successBit&2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_items &&
                        codes[i+2].opcode == OpCodes.Ldc_I4_0 && codes[i+3].opcode == OpCodes.Callvirt && codes[i+4].opcode == OpCodes.Callvirt &&
                        codes[i+5].opcode == OpCodes.Ldc_I4_0 && codes[i+6].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+6].operand == _mi_gameObject_SetActive) {
                            codes[i].labels.Add(tgt_label);
                            successBit|=2;
                    }
                    if ((successBit&4) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i].labels.Count>0 && codes[i+1].opcode == OpCodes.Ldc_I4_0 && 
                        codes[i+2].opcode == OpCodes.Stfld && (FieldInfo)codes[i+2].operand == _fi_weaponIndex) {
                            Label vanilla_label = codes[i].labels[0];
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brfalse, vanilla_label),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldflda, _fi_items),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldflda, _fi_weaponItemPrefabs),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldflda, _fi_charmItemPrefabs),
                                new CodeInstruction(OpCodes.Call, _mi_SetupItems),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Count;
                            successBit|=4;
                    }
                    if (successBit>=7) break;
                }
                if (successBit!=7) throw new Exception($"{nameof(Awake)}: Patch Failed! {successBit}");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static void SetupItems(ref List<ShopSceneItem> items, ref ShopSceneItem[] weaponItemPrefabs, ref ShopSceneItem[] charmItemPrefabs) {
                int wlen = weaponItemPrefabs.Length;
                int clen = charmItemPrefabs.Length;
                ShopSceneItem[] wtmp = new ShopSceneItem[wlen];
                ShopSceneItem[] ctmp = new ShopSceneItem[clen];
                
                for (int i=0; i<wlen; i++) {
                    ShopSceneItem item = weaponItemPrefabs[i];
                    wtmp[weaponOrder[item.weapon]] = item;
                }
                for (int i=0; i<clen; i++) {
                    ShopSceneItem item = charmItemPrefabs[i];
                    ctmp[charmOrder[item.charm]] = item;
                }
                int wnullc = Aux.ArrayNullCount(wtmp);
                int cnullc = Aux.ArrayNullCount(ctmp);
                if (wnullc+cnullc>0) {
                    Plugin.LogError($"Prefabs have null values! w:{wnullc} c:{cnullc}");
                    return;
                }
                int shopIndex = worldIndexes[PlayerData.Data.CurrentMap];
                int weaponCount = APClient.SlotData.shop_map[shopIndex].Weapons;
                int charmCount = APClient.SlotData.shop_map[shopIndex].Charms;
                weaponItemPrefabs = new ShopSceneItem[APClient.SlotData.shop_map[0].Weapons];
                charmItemPrefabs = new ShopSceneItem[APClient.SlotData.shop_map[0].Weapons];
                items.Clear();
                bool charm = false;
                for (int i=0; i<weaponCount+charmCount; i++) {}
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "Start")]
        internal static class Start {
            static bool Prefix(ShopScenePlayer __instance, PlayerId ___player) {
                if (APData.IsCurrentSlotEnabled() && ___player!=0) {
                    __instance.enabled = false;
                    __instance.gameObject.SetActive(false);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "UpdateSelection")]
        internal static class UpdateSelection {}

        [HarmonyPatch(typeof(ShopScenePlayer), "addNewItem_cr", MethodType.Enumerator)]
        internal static class addNewItem_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                byte labelbits = 0;

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                if (labelbits>0) {
                    for (int i=0;i<codes.Count-1;i++) {
                    }
                }
                if (!success||labelbits!=7) throw new Exception($"{nameof(addNewItem_cr)}: Patch Failed! {success}:{labelbits}");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }
        }
    }
}
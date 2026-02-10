/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Util;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopScenePlayerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(UpdateSelection));
            Harmony.CreateAndPatchAll(typeof(Purchase));
            Harmony.CreateAndPatchAll(typeof(addNewItem_cr));
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "Awake")]
        internal static class Awake {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_Multiplayer = typeof(PlayerManager).GetField("Multiplayer", BindingFlags.Public | BindingFlags.Static);
                FieldInfo _fi_player = typeof(ShopScenePlayer).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_items = typeof(ShopScenePlayer).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weaponItemPrefabs = typeof(ShopScenePlayer).GetField("weaponItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_charmItemPrefabs = typeof(ShopScenePlayer).GetField("charmItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weaponIndex = typeof(ShopScenePlayer).GetField("weaponIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_charmIndex = typeof(ShopScenePlayer).GetField("charmIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weapon = typeof(ShopSceneItem).GetField("weapon", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_charm = typeof(ShopSceneItem).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_gameObject_SetActive = typeof(GameObject).GetMethod("SetActive");
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_IsUnlocked_c = GetMethod_IsUnlocked(typeof(Charm));
                MethodInfo _mi_IsUnlocked_w = GetMethod_IsUnlocked(typeof(Weapon));
                MethodInfo _mi_item_IsAvailable = typeof(ShopSceneItem).GetProperty("IsAvailable", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_APMPCondition = typeof(Awake).GetMethod("APMPCondition", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_SetupItems = typeof(Awake).GetMethod("SetupItems", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_IsAPWeaponChecked = typeof(ShopHookBase).GetMethod("IsAPWeaponChecked", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_IsAPCharmChecked = typeof(ShopHookBase).GetMethod("IsAPCharmChecked", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_vanilla = il.DefineLabel();
                Label l_vwwhile = il.DefineLabel();
                Label l_vcwhile = il.DefineLabel();
                Label l_cwwhile = il.DefineLabel();
                Label l_ccwhile = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 11; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == _fi_Multiplayer &&
                        codes[i + 1].opcode == OpCodes.Brtrue && codes[i + 3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 3].operand == _fi_player &&
                        codes[i + 4].opcode == OpCodes.Ldc_I4_1 && codes[i + 5].opcode == OpCodes.Bne_Un
                    ) {
                        codes[i + 1].opcode = OpCodes.Brfalse;
                        codes.Insert(i + 1, new(OpCodes.Call, _mi_APMPCondition));
                        success |= 1;
                    }
                    if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i].labels.Count > 0 && codes[i + 1].opcode == OpCodes.Ldc_I4_0 &&
                        codes[i + 2].opcode == OpCodes.Stfld && (FieldInfo)codes[i + 2].operand == _fi_weaponIndex
                    ) {
                        List<Label> orig_labels = codes[i].labels;
                        codes[i].labels = [l_vanilla];
                        CodeInstruction[] ncodes = [
                            CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                            new(OpCodes.Brfalse, l_vanilla),
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Ldflda, _fi_items),
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Ldflda, _fi_weaponItemPrefabs),
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Ldflda, _fi_charmItemPrefabs),
                            new(OpCodes.Call, _mi_SetupItems),
                        ];
                        ncodes[0].labels = orig_labels;
                        codes.InsertRange(i, ncodes);
                        i += ncodes.Length;
                        success |= 2;
                    }
                    if ((success & 12) != 12 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data &&
                        codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 2].operand == _fi_player &&
                        codes[i + 3].opcode == OpCodes.Ldarg_0 && codes[i + 4].opcode == OpCodes.Ldfld && codes[i + 5].opcode == OpCodes.Ldarg_0 &&
                        codes[i + 6].opcode == OpCodes.Ldfld && codes[i + 7].opcode == OpCodes.Ldelem_Ref && codes[i + 8].opcode == OpCodes.Ldfld &&
                        codes[i + 9].opcode == OpCodes.Callvirt && codes[i + 10].opcode == OpCodes.Brtrue
                    ) {
                        if ((success & 4) == 0 && (FieldInfo)codes[i + 4].operand == _fi_weaponItemPrefabs && (FieldInfo)codes[i + 8].operand == _fi_weapon) {
                            Label tgt_label = (Label)codes[i + 10].operand;
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new(OpCodes.Brfalse, l_vwwhile),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_weaponItemPrefabs),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_weaponIndex),
                                new(OpCodes.Ldelem_Ref),
                                new(OpCodes.Ldfld, _fi_weapon),
                                new(OpCodes.Call, _mi_IsAPWeaponChecked),
                                new(OpCodes.Brfalse, l_cwwhile),
                                new(OpCodes.Br, tgt_label),
                            ];
                            codes[i].labels.Add(l_vwwhile);
                            codes[i + 11].labels.Add(l_cwwhile);
                            codes.InsertRange(i, ncodes);
                            i += ncodes.Length;
                            success |= 4;
                        }
                        else if ((success & 8) == 0 && (FieldInfo)codes[i + 4].operand == _fi_charmItemPrefabs && (FieldInfo)codes[i + 8].operand == _fi_charm) {
                            Label tgt_label = (Label)codes[i + 10].operand;
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new(OpCodes.Brfalse, l_vcwhile),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_charmItemPrefabs),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_charmIndex),
                                new(OpCodes.Ldelem_Ref),
                                new(OpCodes.Ldfld, _fi_charm),
                                new(OpCodes.Call, _mi_IsAPCharmChecked),
                                new(OpCodes.Brfalse, l_ccwhile),
                                new(OpCodes.Br, tgt_label),
                            ];
                            codes[i].labels.Add(l_vcwhile);
                            codes[i + 11].labels.Add(l_ccwhile);
                            codes.InsertRange(i, ncodes);
                            i += ncodes.Length;
                            success |= 8;
                        }
                        else {
                            Logging.LogWarning($"{nameof(Awake)}: Found cluster bit 12, but bits 4 and 8 not found!");
                        }
                    }
                    if (success >= 15) break;
                }
                if (success != 15) throw new Exception($"{nameof(Awake)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APMPCondition(bool origCond) {
                return !origCond || APData.IsCurrentSlotEnabled();
            }
            private static void SetupItems(
                ref List<ShopSceneItem> items,
                ref ShopSceneItem[] weaponItemPrefabs,
                ref ShopSceneItem[] charmItemPrefabs
            ) {
                int wlen = weaponItemPrefabs.Length;
                int clen = charmItemPrefabs.Length;
                ShopSceneItem[] wtmp = new ShopSceneItem[wlen];
                ShopSceneItem[] ctmp = new ShopSceneItem[clen];

                //ShopDebug(items, weaponItemPrefabs, charmItemPrefabs);

                // Setting order of prefabs for all shops
                for (int i = 0; i < wlen; i++) {
                    ShopSceneItem item = weaponItemPrefabs[i];
                    wtmp[ShopHookBase.GetWeaponOrderIndex(item.weapon)] = item;
                }
                for (int i = 0; i < clen; i++) {
                    ShopSceneItem item = charmItemPrefabs[i];
                    ctmp[ShopHookBase.GetCharmOrderIndex(item.charm)] = item;
                }
                int wnullc = Aux.ArrayNullCount(wtmp);
                int cnullc = Aux.ArrayNullCount(ctmp);
                if (wnullc + cnullc > 0) {
                    Logging.LogError($"[ShopScenePlayerHook] Null Prefabs! w:{wnullc} c:{cnullc}");
                    return;
                }

                Logging.Log($"wp {ShopSceneItemsToString(weaponItemPrefabs)}", LoggingFlags.Debug);
                Logging.Log($"cp {ShopSceneItemsToString(charmItemPrefabs)}", LoggingFlags.Debug);
                Logging.Log($"wt {ShopSceneItemsToString(wtmp)}", LoggingFlags.Debug);
                Logging.Log($"ct {ShopSceneItemsToString(ctmp)}", LoggingFlags.Debug);

                int totalItemCount = SetupShopTiers(ref items, ref weaponItemPrefabs, ref charmItemPrefabs, wtmp, ctmp);

                // Set items in shop
                List<ShopSceneItem> nitems = new();
                bool charm = false;
                int wi = 0;
                int ci = 0;
                for (int i = 0; i < Math.Min(totalItemCount, 5); i++) {
                    if (charm) {
                        nitems.Add(charmItemPrefabs[ci]);
                        ci++;
                        charm = false;
                    }
                    else {
                        nitems.Add(weaponItemPrefabs[wi]);
                        wi++;
                        charm = true;

                    }
                }
                Transform parent = items[0].transform.parent;
                foreach (ShopSceneItem item in items) {
                    item.gameObject.SetActive(false);
                    GameObject.Destroy(item.gameObject);
                }
                items.Clear();
                foreach (ShopSceneItem item in nitems) {
                    ShopSceneItem nitem = UnityEngine.Object.Instantiate(item, parent);
                    nitem.name = item.name;
                    items.Add(nitem);
                }

                //ShopDebug(items, weaponItemPrefabs, charmItemPrefabs);
            }
            private static int SetupShopTiers(
                ref List<ShopSceneItem> items,
                ref ShopSceneItem[] weaponItemPrefabs,
                ref ShopSceneItem[] charmItemPrefabs,
                ShopSceneItem[] wtmp,
                ShopSceneItem[] ctmp
            ) {
                ShopSet[] shopMap = ShopMap.GetShopMap();
                Levels[] w2LevelPreqs = [
                    Levels.FlyingGenie, Levels.FlyingBird, Levels.Dragon, Levels.Platforming_Level_2_1, Levels.Platforming_Level_2_2
                ];
                Levels[] w3LevelPreqs = [
                    Levels.SallyStagePlay, Levels.Platforming_Level_3_1, Levels.Platforming_Level_3_2
                ];

                int shopLevel = 0;
                if (Ext.CheckAnyLevelComplete(w2LevelPreqs.LMapped()) || (APSettings.FreemoveIsles &&
                    PlayerData.Data.GetMapData(Scenes.scene_map_world_2).sessionStarted)
                ) {
                    shopLevel++;
                }
                if (Ext.CheckAnyLevelComplete(w3LevelPreqs.LMapped()) || (APSettings.FreemoveIsles &&
                        PlayerData.Data.GetMapData(Scenes.scene_map_world_3).sessionStarted)
                ) {
                    shopLevel++;
                }
                if (APSettings.UseDLC && PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).sessionStarted) {
                    shopLevel++;
                }

                Logging.Log($"ShopLevel: {shopLevel}");

                // Setting up counts
                int weaponCount = 0;
                int charmCount = 0;

                for (int i = 0; i <= shopLevel; i++) {
                    weaponCount += shopMap[i].Weapons;
                    charmCount += shopMap[i].Charms;
                }

                int totalCount = weaponCount + charmCount;

                Logging.Log($"wc {weaponCount}", LoggingFlags.Debug);
                Logging.Log($"cc {charmCount}", LoggingFlags.Debug);

                // Set prefabs for shop
                weaponItemPrefabs = Aux.ArrayRange(wtmp, weaponCount);
                charmItemPrefabs = Aux.ArrayRange(ctmp, charmCount);
                //HashSet<Weapon> weaponSet = GetWeaponSet(weaponItemPrefabs);
                //HashSet<Charm> charmSet = GetCharmSet(charmItemPrefabs);

                Logging.Log($"wps {ShopSceneItemsToString(weaponItemPrefabs)}", LoggingFlags.Debug);
                Logging.Log($"cps {ShopSceneItemsToString(charmItemPrefabs)}", LoggingFlags.Debug);

                return totalCount;
            }
            private static HashSet<Weapon> GetWeaponSet(IEnumerable<ShopSceneItem> items) {
                HashSet<Weapon> wset = [];
                foreach (ShopSceneItem item in items) {
                    if (!wset.Contains(item.weapon))
                        wset.Add(item.weapon);
                    else
                        Logging.LogWarning($"[SetupItems] {item.weapon} already exists in set!");
                }
                return wset;
            }
            private static HashSet<Charm> GetCharmSet(IEnumerable<ShopSceneItem> items) {
                HashSet<Charm> cset = [];
                foreach (ShopSceneItem item in items) {
                    if (!cset.Contains(item.charm))
                        cset.Add(item.charm);
                    else
                        Logging.LogWarning($"[SetupItems] {item.charm} already exists in set!");
                }
                return cset;
            }
            private static string ShopSceneItemsToString(IEnumerable<ShopSceneItem> items) {
                bool first = true;
                string res = "[";
                foreach (var item in items) {
                    string prefix = !first ? ", " : "";
                    if (first) first = false;
                    res += prefix + ((item.itemType == ItemType.Charm) ? item.charm : item.weapon);
                }
                res += "]";
                return res;
            }
            private static void ShopDebug(
                List<ShopSceneItem> items,
                ShopSceneItem[] weaponItemPrefabs,
                ShopSceneItem[] charmItemPrefabs
            ) {
                if (Logging.IsDebugEnabled()) {
                    Logging.Log("---SHOP---", LoggingFlags.Debug);
                    Logging.Log("-items-", LoggingFlags.Debug);
                    foreach (ShopSceneItem item in items)
                        Logging.Log(item.ToString(), LoggingFlags.Debug);
                    Logging.Log("", LoggingFlags.Debug);
                    Logging.Log("-charmItemPrefabs-", LoggingFlags.Debug);
                    foreach (ShopSceneItem item in charmItemPrefabs)
                        Logging.Log(item.ToString(), LoggingFlags.Debug);
                    Logging.Log("", LoggingFlags.Debug);
                    Logging.Log("-weaponItemPrefabs-", LoggingFlags.Debug);
                    foreach (ShopSceneItem item in weaponItemPrefabs)
                        Logging.Log(item.ToString(), LoggingFlags.Debug);
                    Logging.Log("", LoggingFlags.Debug);
                    Logging.Log("---END SHOP---", LoggingFlags.Debug);
                }
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "Start")]
        internal static class Start {
            static bool Prefix(ShopScenePlayer __instance, PlayerId ___player) {
                if (APData.IsCurrentSlotEnabled() && ___player != 0) {
                    __instance.enabled = false;
                    __instance.gameObject.SetActive(false);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "UpdateSelection")]
        internal static class UpdateSelection {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;

                Label end = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                codes[codes.Count - 1].labels.Add(end);
                for (int i = 0; i < codes.Count - 4; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 3].opcode == OpCodes.Ldc_I4 &&
                        (Charm)codes[i + 3].operand == Charm.charm_curse && codes[i + 4].opcode == OpCodes.Bne_Un
                    ) {
                        CodeInstruction[] ncodes = [
                            CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                            new CodeInstruction(OpCodes.Brtrue, end),
                        ];
                        codes.InsertRange(i, ncodes);
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateSelection)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "Purchase")]
        internal static class Purchase {
            static void Postfix() {
                if (ShopHookBase.APIsAllItemsBought()) {
                    APClient.GoalComplete(Goals.ShopBuyout, true);
                }
            }
        }

        [HarmonyPatch(typeof(ShopScenePlayer), "addNewItem_cr", MethodType.Enumerator)]
        internal static class addNewItem_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                Type crtype = Reflection.GetEnumeratorType(
                    typeof(ShopScenePlayer).GetMethod("addNewItem_cr", BindingFlags.NonPublic | BindingFlags.Instance)
                );
                FieldInfo _fi_this = crtype.GetField("$this", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_i2 = crtype.GetField("<i>__2", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_i6 = crtype.GetField("<i>__6", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_IsUnlocked_c = GetMethod_IsUnlocked(typeof(Charm));
                MethodInfo _mi_IsUnlocked_w = GetMethod_IsUnlocked(typeof(Weapon));
                FieldInfo _fi_charmItemPrefabs = typeof(ShopScenePlayer).GetField("charmItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_weaponItemPrefabs = typeof(ShopScenePlayer).GetField("weaponItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_charm = typeof(ShopSceneItem).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_weapon = typeof(ShopSceneItem).GetField("weapon", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_IsAPCharmChecked = typeof(ShopHookBase).GetMethod("IsAPCharmChecked", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_IsAPWeaponChecked = typeof(ShopHookBase).GetMethod("IsAPWeaponChecked", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_cvanilla = il.DefineLabel();
                Label l_wvanilla = il.DefineLabel();
                Label l_cifa = il.DefineLabel();
                Label l_wifa = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 13; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i + 2].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i + 2].operand == _fi_this && codes[i + 11].opcode == OpCodes.Callvirt && codes[i + 12].opcode == OpCodes.Brtrue) {
                        Label l_contloop = (Label)codes[i + 12].operand;
                        if ((success & 1) == 0 && (MethodInfo)codes[i + 11].operand == _mi_IsUnlocked_c) {
                            codes[i].labels.Add(l_cvanilla);
                            codes[i + 13].labels.Add(l_cifa);
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new(OpCodes.Brfalse, l_cvanilla),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_this),
                                new(OpCodes.Ldfld, _fi_charmItemPrefabs),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_i2),
                                new(OpCodes.Ldelem_Ref),
                                new(OpCodes.Ldfld, _fi_charm),
                                new(OpCodes.Call, _mi_IsAPCharmChecked),
                                new(OpCodes.Brtrue, l_contloop),
                                new(OpCodes.Br, l_cifa),
                            ];
                            codes.InsertRange(i, ncodes);
                            i += ncodes.Length;
                            success |= 1;
                        }
                        else if ((success & 2) == 0 && (MethodInfo)codes[i + 11].operand == _mi_IsUnlocked_w) {
                            codes[i].labels.Add(l_wvanilla);
                            codes[i + 13].labels.Add(l_wifa);
                            CodeInstruction[] ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_this),
                                new(OpCodes.Ldfld, _fi_weaponItemPrefabs),
                                new(OpCodes.Ldarg_0),
                                new(OpCodes.Ldfld, _fi_i6),
                                new(OpCodes.Ldelem_Ref),
                                new(OpCodes.Ldfld, _fi_weapon),
                                new(OpCodes.Call, _mi_IsAPWeaponChecked),
                                new(OpCodes.Brtrue, l_contloop),
                                new(OpCodes.Br, l_wifa),
                            ];
                            codes.InsertRange(i, ncodes);
                            i += ncodes.Length;
                            success |= 2;
                        }
                        else Logging.LogError($"{nameof(addNewItem_cr)}: Cannot find IsUnlocked method!");
                    }
                }
                if (success != 3) throw new Exception($"{nameof(addNewItem_cr)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
        }

        private static MethodInfo GetMethod_IsUnlocked(Type type) {
            return typeof(PlayerData).GetMethod("IsUnlocked", BindingFlags.Public | BindingFlags.Instance,
                null, [typeof(PlayerId), type], null);
        }
    }
}

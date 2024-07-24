/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopSceneItemHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(isPurchased));
        }

        private static readonly Dictionary<Weapon, APLocation> weaponLocations = new() {
            {Weapon.level_weapon_homing, APLocation.shop_weapon1},
            {Weapon.level_weapon_spreadshot, APLocation.shop_weapon2},
            {Weapon.level_weapon_boomerang, APLocation.shop_weapon3},
            {Weapon.level_weapon_bouncer, APLocation.shop_weapon4},
            {Weapon.level_weapon_charge, APLocation.shop_weapon5},
            {Weapon.level_weapon_wide_shot, APLocation.shop_dlc_weapon6},
            {Weapon.level_weapon_crackshot, APLocation.shop_dlc_weapon7},
            {Weapon.level_weapon_upshot, APLocation.shop_dlc_weapon8},
        };
        private static readonly Dictionary<Charm, APLocation> charmLocations = new() {
            {Charm.charm_health_up_1, APLocation.shop_charm1},
            {Charm.charm_smoke_dash, APLocation.shop_charm2},
            {Charm.charm_parry_plus, APLocation.shop_charm3},
            {Charm.charm_super_builder, APLocation.shop_charm4},
            {Charm.charm_parry_attack, APLocation.shop_charm5},
            {Charm.charm_health_up_2, APLocation.shop_charm6},
            {Charm.charm_healer, APLocation.shop_dlc_charm7},
            {Charm.charm_curse, APLocation.shop_dlc_charm8},
        };

        [HarmonyPatch(typeof(ShopSceneItem), "isPurchased")]
        internal static class isPurchased {
            static bool Prefix(ref bool __result, ItemType ___itemType, Weapon ___weapon, Charm ___charm) {
                if (APData.IsCurrentSlotEnabled()) {
                    long loc = -1;
                    if (___itemType==ItemType.Weapon) {
                        if (weaponLocations.ContainsKey(___weapon))
                            loc = weaponLocations[___weapon];
                        else
                            Plugin.LogWarning($"[ShopSceneItem] Unknown item: {___weapon}");
                    }
                    else if (___itemType==ItemType.Charm) {
                        if (charmLocations.ContainsKey(___charm))
                            loc = charmLocations[___charm];
                        else
                            Plugin.LogWarning($"[ShopSceneItem] Unknown item: {___charm}");
                    }
                    else
                        Plugin.LogWarning("[ShopSceneItem] Unexpected type");
                    __result = APClient.IsLocationChecked(loc);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "Purchase")]
        internal static class Purchase {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                byte labelbits = 0;
                FieldInfo _fi_itemType = typeof(ShopSceneItem).GetField("itemType", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_weapon = typeof(ShopSceneItem).GetField("weapon", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_charm = typeof(ShopSceneItem).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(Purchase).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label vanilla_label = il.DefineLabel();
                Label flag_label = il.DefineLabel();
                Label end_label = new();

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                if (labelbits>0) {
                    for (int i=codes.Count-2;i>=0;i--) {
                        if (labelbits==0 && codes[i+1].opcode == OpCodes.Ret && codes[i].labels.Count>0) {
                            end_label = codes[i].labels[0];
                            labelbits|=1;
                        }
                        else if ((labelbits&2)==0 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i+1].opcode == OpCodes.Brfalse && (Label)codes[i+1].operand == end_label) {
                            codes[i].labels.Add(flag_label);
                            labelbits|=2;
                        }
                        else if ((labelbits&4)==0 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i+1].opcode == OpCodes.Stloc_0) {
                            if ((i+2)<codes.Count) {
                                codes[i+2].labels.Add(vanilla_label);
                                labelbits|=4;
                                List<CodeInstruction> ncodes = [
                                    CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                    new CodeInstruction(OpCodes.Brfalse, vanilla_label),
                                    new CodeInstruction(OpCodes.Ldfld, _fi_itemType),
                                    new CodeInstruction(OpCodes.Ldfld, _fi_weapon),
                                    new CodeInstruction(OpCodes.Ldfld, _fi_charm),
                                    new CodeInstruction(OpCodes.Call, _mi_APCheck),
                                    new CodeInstruction(OpCodes.Stloc_0),
                                    new CodeInstruction(OpCodes.Br, flag_label),
                                ];
                                codes.InsertRange(i+2, ncodes);
                                success = true;
                            }
                        }
                    }
                }
                if (!success||labelbits!=7) throw new Exception($"{nameof(Purchase)}: Patch Failed! {success}:{labelbits}");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static bool APCheck(ItemType itemType, Weapon weapon, Charm charm) {
                bool res = false;
                switch(itemType) {
                    case ItemType.Weapon:
                        if (weaponLocations.ContainsKey(weapon)) {
                            long loc = weaponLocations[weapon];
                            if (!APClient.IsLocationChecked(loc)) {
                                APClient.Check(loc);
                                res = true;
                            }
                        }
                        else Plugin.LogWarning($"[ShopSceneItem] Unknown item: {weapon}");
                        break;
                    case ItemType.Charm: 
                        if (charmLocations.ContainsKey(charm)) {
                            long loc = charmLocations[charm];
                            if (!APClient.IsLocationChecked(loc)) {
                                APClient.Check(loc);
                                res = true;
                            }
                        }
                        else Plugin.LogWarning($"[ShopSceneItem] Unknown item: {charm}");
                        break;
                    default:
                        Plugin.LogWarning($"[ShopSceneItem] Cannot Check item. Unknown Type {itemType}");
                        break;
                }
                return res;
            }
        }
    }
}
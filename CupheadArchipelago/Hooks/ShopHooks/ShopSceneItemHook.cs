/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopSceneItemHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Description));
            Harmony.CreateAndPatchAll(typeof(DisplayName));
            Harmony.CreateAndPatchAll(typeof(Subtext));
            Harmony.CreateAndPatchAll(typeof(Value));
            Harmony.CreateAndPatchAll(typeof(isPurchased));
            Harmony.CreateAndPatchAll(typeof(Purchase));
        }

        internal static bool IsAPLocationChecked(ShopSceneItem item) {
            return APClient.IsLocationChecked(ShopMap.GetAPLocation(item));
        }

        [HarmonyPatch(typeof(ShopSceneItem), "Init")]
        internal static class Init {}

        [HarmonyPatch(typeof(ShopSceneItem), "Description", MethodType.Getter)]
        internal static class Description {
            static bool Prefix(ShopSceneItem __instance, ref string __result) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                long loc = ShopMap.GetAPLocation(__instance);
                if (loc<0) {
                    __result = $"{__instance.itemType}";
                    return false;
                }
                ScoutedItemInfo check = APClient.GetCheck(loc);
                if (check != null) {
                    string res = check.ItemName ?? $"#{check.ItemId}";
                    if (res.Length>64)
                        res = $"{res.Substring(0, 64)}...";
                    __result = res;
                }
                else __result = "Missing Location Scout Data.";

                return false;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "DisplayName", MethodType.Getter)]
        internal static class DisplayName {
            static bool Prefix(ShopSceneItem __instance, ref string __result) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                long loc = ShopMap.GetAPLocation(__instance);
                if (loc<0) {
                    __result = "INVALID";
                    return false;
                }
                ScoutedItemInfo check = APClient.GetCheck(loc);
                if (check != null) {
                    string sres = "";
                    if ((check.Flags&ItemFlags.Advancement)>0)
                        sres = "P";
                    else if ((check.Flags&ItemFlags.NeverExclude)>0)
                        sres = "U";
                    if ((check.Flags&ItemFlags.Trap)>0)
                        sres = $"{sres}T";
                    if (sres.Length==0)
                        sres = "F";
                    string name = $"AP ITEM ({sres})";
                    __result = name ?? $"APItem {check.ItemId}";
                }
                else __result = $"AP{loc}";

                return false;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "Subtext", MethodType.Getter)]
        internal static class Subtext {
            static bool Prefix(ShopSceneItem __instance, ref string __result) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                long loc = ShopMap.GetAPLocation(__instance);
                if (loc<0) {
                    __result = "Unknown type";
                    return false;
                }
                ScoutedItemInfo check = APClient.GetCheck(loc);
                if (check != null) {
                    __result = $"For {check.Player}";
                }
                else __result = "Missing Location Scout Data.";

                return false;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "Value", MethodType.Getter)]
        internal static class Value {
            static bool Prefix(ItemType ___itemType, ref int __result) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                
                // Static until pricing is randomizable
                if (___itemType == ItemType.Charm) __result = 3;
                else __result = 4;

                return false;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "isPurchased")]
        internal static class isPurchased {
            static bool Prefix(ShopSceneItem __instance, ref bool __result) {
                if (APData.IsCurrentSlotEnabled()) {
                    __result = IsAPLocationChecked(__instance);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ShopSceneItem), "Purchase")]
        internal static class Purchase {
            //FIXME: Better quality
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                byte success = 0;
                
                FieldInfo _fi_itemType = typeof(ShopSceneItem).GetField("itemType", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_weapon = typeof(ShopSceneItem).GetField("weapon", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_charm = typeof(ShopSceneItem).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(Purchase).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label vanilla_label = il.DefineLabel();
                Label flag_label = il.DefineLabel();
                Label end_label = new();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=codes.Count-2;i>=0;i--) {
                    if (success==0 && codes[i+1].opcode == OpCodes.Ret && codes[i].labels.Count>0) {
                        end_label = codes[i].labels[0];
                        success|=1;
                    }
                    else if ((success&2)==0 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i+1].opcode == OpCodes.Brfalse && (Label)codes[i+1].operand == end_label) {
                        codes[i].labels.Add(flag_label);
                        success|=2;
                    }
                    else if ((success&4)==0 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i+1].opcode == OpCodes.Stloc_0) {
                        if ((i+2)<codes.Count) {
                            codes[i+2].labels.Add(vanilla_label);
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brfalse, vanilla_label),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Call, _mi_APCheck),
                                new CodeInstruction(OpCodes.Stloc_0),
                                new CodeInstruction(OpCodes.Br, flag_label),
                            ];
                            codes.InsertRange(i+2, ncodes);
                            i+=ncodes.Count;
                            success|=4;
                        }
                    }
                }
                if (success!=7) throw new Exception($"{nameof(Purchase)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            // TODO: Eventually use transpipler which is more elegant.
            static void Postfix(ShopSceneItem __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (PlayerData.Data.shouldShowShopkeepTooltip) {
                        PlayerData.Data.shouldShowShopkeepTooltip = false;
                    }
                    if (ShopScene.Current.HasBoughtEverythingForAchievement(__instance.player)) {
                        APClient.GoalComplete(Goals.ShopBuyout, true);
                    }
                    APClient.SendChecks(true);
                }
            }

            private static bool APCheck(ShopSceneItem item) {
                bool res = false;
                long loc = ShopMap.GetAPLocation(item);
                if (!APClient.IsLocationChecked(loc) && PlayerData.Data.GetCurrency(0) >= item.Value) {
                    PlayerData.Data.AddCurrency(0, -item.Value);
                    APClient.Check(loc, false);
                    res = true;
                }
                return res;
            }
        }
    }
}

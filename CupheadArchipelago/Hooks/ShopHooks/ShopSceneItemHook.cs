/// Copyright 2025-2026 JKLeckr
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
        internal static class Init { }

        [HarmonyPatch(typeof(ShopSceneItem), "Description", MethodType.Getter)]
        internal static class Description {
            static bool Prefix(ShopSceneItem __instance, ref string __result) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                long loc = ShopMap.GetAPLocation(__instance);
                if (loc < 0) {
                    __result = $"{__instance.itemType}";
                    return false;
                }
                ScoutedItemInfo check = APClient.GetCheck(loc);
                if (check != null) {
                    string res = check.ItemName ?? $"#{check.ItemId}";
                    if (res.Length > 64)
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
                if (loc < 0) {
                    __result = "INVALID";
                    return false;
                }
                ScoutedItemInfo check = APClient.GetCheck(loc);
                if (check != null) {
                    string sres = "";
                    if ((check.Flags & ItemFlags.Advancement) > 0)
                        sres = "P";
                    else if ((check.Flags & ItemFlags.NeverExclude) > 0)
                        sres = "U";
                    if ((check.Flags & ItemFlags.Trap) > 0)
                        sres = $"{sres}T";
                    if (sres.Length == 0)
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
                if (loc < 0) {
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
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                byte success = 0;

                FieldInfo _fi_itemType = typeof(ShopSceneItem).GetField("itemType", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_weapon = typeof(ShopSceneItem).GetField("weapon", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_charm = typeof(ShopSceneItem).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APCheck = typeof(Purchase).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_vanilla = il.DefineLabel();
                Label l_flag = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = codes.Count - 5; i >= 0; i--) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i + 1].operand == _fi_itemType && codes[i + 2].opcode == OpCodes.Stloc_1 &&
                        codes[i + 3].opcode == OpCodes.Ldloc_1 && codes[i + 4].opcode == OpCodes.Switch
                    ) {
                        codes[i].labels.Add(l_vanilla);
                        CodeInstruction[] ncodes = [
                            CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                            new(OpCodes.Brfalse, l_vanilla),
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Call, _mi_APCheck),
                            new(OpCodes.Stloc_0),
                            new(OpCodes.Br, l_flag),
                        ];
                        codes.InsertRange(i, ncodes);
                        i += ncodes.Length;
                        success |= 1;
                    }
                    if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Brfalse &&
                        codes[i + 2].opcode == OpCodes.Ldarg_0
                    ) {
                        codes[i].labels.Add(l_flag);
                        success |= 2;
                    }
                    if (success == 3) break;
                }
                if (success != 3) throw new Exception($"{nameof(Purchase)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            static void Postfix(ShopSceneItem __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (PlayerData.Data.shouldShowShopkeepTooltip) {
                        PlayerData.Data.shouldShowShopkeepTooltip = false;
                    }
                    APClient.SendChecks(true);
                }
            }

            private static bool APCheck(ShopSceneItem item) {
                bool res = false;
                long loc = ShopMap.GetAPLocation(item);
                if (APClient.IsLocationChecked(loc)) {
                    res = true;
                }
                else if (PlayerData.Data.GetCurrency(0) >= item.Value) {
                    PlayerData.Data.AddCurrency(0, -item.Value);
                    Logging.Log($"Current coins: {PlayerData.Data.GetCurrency(PlayerId.PlayerOne)}");
                    Logging.Log($"Total coins: {APClient.APSessionGSPlayerData.coins_collected}");
                    APClient.Check(loc, false);
                    res = true;
                }
                return res;
            }
        }
    }
}

/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Auxiliary;
using HarmonyLib;
using static PlayerData;

namespace CupheadArchipelago.Hooks {
    internal class PlayerDataHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(ClearSlot));
            Harmony.CreateAndPatchAll(typeof(OnLoaded));
            Harmony.CreateAndPatchAll(typeof(SaveCurrentFile));
            Harmony.CreateAndPatchAll(typeof(AddCurrency));
            Harmony.CreateAndPatchAll(typeof(ApplyLevelCoins));
            //Harmony.CreateAndPatchAll(typeof(NumWeapons));
            //PlayerInventoryHook.Hook();
            //PlayerCoinManagerHook.Hook();
        }

        private static bool _APMode = false;

        [HarmonyPatch(typeof(PlayerData), "ClearSlot")]
        internal static class ClearSlot {
            private static readonly FieldInfo _fi_inventories = typeof(PlayerData).GetField("inventories", BindingFlags.NonPublic | BindingFlags.Instance);

            static bool Prefix(int slot, PlayerData[] ____saveFiles) {
                // Sanitize Slot of vanilla defaults and replace with AP Defaults
                if (_APMode) {
                    PlayerData data = ____saveFiles[slot];
                    PlayerInventories inventories = (PlayerInventories)_fi_inventories.GetValue(data);
                    Weapon weapon = ItemMap.GetWeapon(APSettings.StartWeapon);
                    inventories.playerOne._weapons = [weapon];
                    data.Loadouts.playerOne.primaryWeapon = weapon;
                    inventories.playerTwo._weapons = [weapon];
                    data.Loadouts.playerTwo.primaryWeapon = weapon;
                    return false;
                }
                else return true;
            }
        }

        internal static void APSanitizeSlot(int slot) {
            _APMode = true;
            ClearSlot(slot);
            _APMode = false;
        }

        [HarmonyPatch(typeof(PlayerData), "OnLoaded")]
        internal static class OnLoaded {
            static void Postfix() {
                Plugin.Log("Loading...");
                APData.LoadData();
            }
        }

        [HarmonyPatch(typeof(PlayerData), "SaveCurrentFile")]
        internal static class SaveCurrentFile {
            static void Postfix() {
                int index = CurrentSaveFileIndex;
                Debug.Log($"[APData] Saving to slot {index}");
                APData.Save(index);
            }
        }

        [HarmonyPatch(typeof(PlayerData), "AddCurrency")]
        internal static class AddCurrency {
            static bool Prefix() {
                Plugin.Log("AddCurrency.");
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "ApplyLevelCoins")]
        internal static class ApplyLevelCoins {
            static bool Prefix(PlayerCoinManager ___coinManager, ref PlayerCoinManager ___levelCoinManager) {
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log("[ApplyLevelCoins] Disabled.");
                    ___levelCoinManager = new PlayerCoinManager();
                    return false;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "NumWeapons")]
        internal static class NumWeapons {
            static void Postfix(PlayerId player, PlayerInventories ___inventories) {
                Plugin.Log(Aux.CollectionToString(___inventories.GetPlayer(player)._weapons));
            }
        }

        private static class PlayerInventoryHook {
            internal static void Hook() {
                Harmony.CreateAndPatchAll(typeof(PlayerInventory));
            }

            [HarmonyPatch(typeof(PlayerInventory), MethodType.Constructor)]
            internal static class PlayerInventory {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    bool debug = false;
                    bool success = false;
                    FieldInfo _fi__weapons = typeof(PlayerInventory).GetField("_weapons", BindingFlags.Public | BindingFlags.Instance);
                    MethodInfo _mi__weapons_Add = typeof(PlayerInventory).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

                    Label vanilla_label = il.DefineLabel();

                    if (debug) {
                        for (int i = 0; i < codes.Count; i++) {
                            Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                        }
                    }
                    for (int i = 0; i < codes.Count - 3; i++) {
                        if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi__weapons &&
                            codes[i+2].opcode == OpCodes.Ldc_I4 && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi__weapons_Add) {
                                codes[i].labels.Add(vanilla_label);
                                List<CodeInstruction> ncodes = [
                                    CodeInstruction.Call(() => IsAPMode()), 
                                    new CodeInstruction(OpCodes.Brfalse, vanilla_label),
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldfld, _fi__weapons),
                                    CodeInstruction.Call(() => GetAPStartWeapon()),
                                    new CodeInstruction(OpCodes.Callvirt, _mi__weapons_Add),
                                    new CodeInstruction(OpCodes.Ret),
                                ];
                                codes.InsertRange(i, ncodes);
                                success = true;
                                break;
                        }
                    }
                    if (!success) throw new Exception($"{nameof(PlayerInventory)}: Patch Failed!");
                    if (debug) {
                        Plugin.Log("---");
                        for (int i = 0; i < codes.Count; i++) {
                            Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                        }
                    }

                    return codes;
                }

                private static bool IsAPMode() => _APMode;
                private static Weapon GetAPStartWeapon() => ItemMap.GetWeapon(APSettings.StartWeapon);
                private static void AddAPWeapon(List<Weapon> weapons) {
                    Weapon weapon = ItemMap.GetWeapon(APSettings.StartWeapon);
                    weapons.Add(weapon);
                }
            }
        }

        private static class PlayerCoinManagerHook {
            internal static void Hook() {
                Harmony.CreateAndPatchAll(typeof(GetCoinCollected));
            }

            [HarmonyPatch(typeof(PlayerCoinManager), "GetCoinCollected", new Type[] {typeof(string)})]
            internal static class GetCoinCollected {
                private static MethodInfo _mi_GetCoin__str = typeof(PlayerCoinManager).GetMethod("GetCoin",
                        BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]{typeof(string)}, null);

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    bool found = false;

                    for (int i=0;i<codes.Count-1;i++) {
                        if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_GetCoin__str) {
                            codes[i] = CodeInstruction.Call(typeof(GetCoinCollected), "APGetCoinCollected"); //FIXME: Style use OpCodes.Call instead
                            codes.RemoveAt(i+1);
                            found = true;
                            break;
                        }
                    }
                    if (found) Plugin.Log("[GetCoinCollected] Successfully patched", LoggingFlags.Transpiler);

                    /*foreach (CodeInstruction code in codes)
                        Console.WriteLine(code);*/

                    return codes;
                }

                private static bool APGetCoinCollected(PlayerCoinManager instance, string coinID) {
                    return ((PlayerCoinProperties)_mi_GetCoin__str.Invoke(instance, new object[]{coinID})).collected;
                }
            }
        }
    }
}

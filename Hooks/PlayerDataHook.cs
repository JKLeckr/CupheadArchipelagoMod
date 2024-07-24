/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System;
using System.Reflection;
using HarmonyLib;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CupheadArchipelago.Hooks {
    public class PlayerDataHook {
        public static void Hook() {
            APData.Init();
            //Harmony.CreateAndPatchAll(typeof(Init));
            Harmony.CreateAndPatchAll(typeof(OnLoaded));
            Harmony.CreateAndPatchAll(typeof(SaveCurrentFile));
            Harmony.CreateAndPatchAll(typeof(AddCurrency));
            Harmony.CreateAndPatchAll(typeof(ApplyLevelCoins));
            //PlayerCoinManagerHook.Hook();
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
                int index = PlayerData.CurrentSaveFileIndex;
                Debug.Log($"[APData] Saving to slot {index}");
                APData.Save(index);
            }
        }

        [HarmonyPatch(typeof(PlayerData), "AddCurrency")]
        internal static class AddCurrency {
            static bool Prefix() {
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log("[AddCurrency] Disabled.");
                    return false;
                } else return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "ApplyLevelCoins")]
        internal static class ApplyLevelCoins {
            static bool Prefix(ref PlayerData.PlayerCoinManager ___levelCoinManager) {
                if (APData.IsCurrentSlotEnabled()) {
                    Plugin.Log("[ApplyLevelCoins] Disabled.");
                    ___levelCoinManager = new PlayerData.PlayerCoinManager();
                    return false;
                } else return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool found = false;
                FieldInfo _fi_collected = typeof(PlayerData.PlayerCoinProperties).GetField("collected", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_AddCurrency = typeof(PlayerData).GetMethod("AddCurrency", BindingFlags.Public | BindingFlags.Instance);

                /*foreach (CodeInstruction code in codes)
                        Console.WriteLine(code);*/

                int target = -1;
                int exitLabelIndex = -1;
                Label origCollectedLabel = il.DefineLabel();
                for (int i=0;i<codes.Count-3;i++) {
                    if (target<0 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_collected && codes[i+2].opcode == OpCodes.Brfalse) {
                        exitLabelIndex = i+2;
                        target = i+3;
                        i+=3;
                    }
                    if (!found && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldc_I4_0 && codes[i+3].operand == _mi_AddCurrency) {
                        codes[i].labels.Add(origCollectedLabel);
                        found = true;
                    }
                }
                if (found && target>=0) {
                    Label exitLabel = (Label)codes[exitLabelIndex].operand;
                    codes.Insert(target, CodeInstruction.Call(() => APEnabled()));
                    codes.Insert(target+1, new CodeInstruction(OpCodes.Brfalse, origCollectedLabel));
                    codes.Insert(target+2, new CodeInstruction(OpCodes.Ldloc_0));
                    codes.Insert(target+3, CodeInstruction.Call(typeof(ApplyLevelCoins), "APCoinCheck"));
                    codes.Insert(target+4, new CodeInstruction(OpCodes.Br, exitLabel));
                } else throw new Exception($"{nameof(ApplyLevelCoins)}: Patch Failed!");

                /*foreach (CodeInstruction code in codes)
                        Console.WriteLine(code);*/

                return codes;
            }
            private static bool APEnabled() => APData.IsCurrentSlotEnabled();
            private static void APCoinCheck(PlayerData.PlayerCoinProperties coin) {
                Console.WriteLine("Send coin check");
            }
        }

        internal static class PlayerCoinManagerHook {
            internal static void Hook() {
                Harmony.CreateAndPatchAll(typeof(GetCoinCollected));
            }

            [HarmonyPatch(typeof(PlayerData.PlayerCoinManager), "GetCoinCollected", new Type[] {typeof(string)})]
            internal static class GetCoinCollected {
                private static MethodInfo _mi_GetCoin__str;

                static GetCoinCollected() {
                    _mi_GetCoin__str = typeof(PlayerData.PlayerCoinManager).GetMethod("GetCoin",
                        BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]{typeof(string)}, null);
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    bool found = false;

                    for (int i=0;i<codes.Count-1;i++) {
                        if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_GetCoin__str) {
                            codes[i] = CodeInstruction.Call(typeof(GetCoinCollected), "APGetCoinCollected");
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

                private static bool APGetCoinCollected(PlayerData.PlayerCoinManager instance, string coinID) {
                    return ((PlayerData.PlayerCoinProperties)_mi_GetCoin__str.Invoke(instance, new object[]{coinID})).collected;
                }
            }
        }
    }
}
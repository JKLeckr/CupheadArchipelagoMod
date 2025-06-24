/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Resources;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks {
    internal class AssetBundleLoaderHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(loadAssetBundle));
            Harmony.CreateAndPatchAll(typeof(loadAssetBundleSynchronous));
        }

        [HarmonyPatch(typeof(AssetBundleLoader), "loadAssetBundle", MethodType.Enumerator)]
        internal static class loadAssetBundle {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                MethodInfo _mi_LoadFromFileAsync = typeof(AssetBundle).GetMethod(
                    "LoadFromFileAsync",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(string)],
                    null
                );
                MethodInfo _mi_IsResourceAsset = typeof(AssetBundleLoaderHook).GetMethod(
                    "IsResourceAsset",
                    BindingFlags.NonPublic | BindingFlags.Static
                );
                MethodInfo _mi_LoadAssetBundleFromResource = typeof(loadAssetBundle).GetMethod(
                    "LoadAssetBundleFromResourceAsync",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                Label l_rjump = il.DefineLabel();
                Label l_ab_stloc = il.DefineLabel();
            
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                // Body here
                if (success == 3) throw new Exception($"{nameof(loadAssetBundle)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }
            
                return codes;
            }

            private static AssetBundleCreateRequest LoadAssetBundleFromResourceAsync(string assetBundle) {
                return AssetBundle.LoadFromMemoryAsync(ResourceLoader.GetLoadedResource(assetBundle));
            }
        }

        [HarmonyPatch(typeof(AssetBundleLoader), "loadAssetBundleSynchronous")]
        internal static class loadAssetBundleSynchronous {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_LoadFromFile = typeof(AssetBundle).GetMethod(
                    "LoadFromFile",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(string)],
                    null
                );
                MethodInfo _mi_IsResourceAsset = typeof(AssetBundleLoaderHook).GetMethod(
                    "IsResourceAsset",
                    BindingFlags.NonPublic | BindingFlags.Static
                );
                MethodInfo _mi_LoadAssetBundleFromResource = typeof(loadAssetBundleSynchronous).GetMethod(
                    "LoadAssetBundleFromResource",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                Label l_rjump = il.DefineLabel();
                Label l_ab_stloc = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                CodeInstruction[] ncodes = [
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, _mi_IsResourceAsset),
                    new(OpCodes.Brtrue, l_rjump),
                ];
                codes.InsertRange(0, ncodes);
                for (int i = ncodes.Length; i < codes.Count - 2; i++) {
                    if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_LoadFromFile &&
                        codes[i + 2].opcode == OpCodes.Stloc_1
                    ) {
                        CodeInstruction[] ncodes2 = [
                            new(OpCodes.Br, l_ab_stloc),
                            new(OpCodes.Ldarg_1),
                            new(OpCodes.Call, _mi_LoadAssetBundleFromResource)
                        ];
                        ncodes2[1].labels.Add(l_rjump);
                        codes[i + 2].labels.Add(l_ab_stloc);
                        codes.InsertRange(i + 2, ncodes2);
                        success = true;
                    }
                }
                if (!success) throw new Exception($"{nameof(loadAssetBundleSynchronous)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static AssetBundle LoadAssetBundleFromResource(string assetBundle) {
                return AssetBundle.LoadFromMemory(ResourceLoader.GetLoadedResource(assetBundle));
            }
        }

        private static bool IsResourceAsset(string assetBundle) => ResourceMap.ResourceExists(assetBundle);
    }
}

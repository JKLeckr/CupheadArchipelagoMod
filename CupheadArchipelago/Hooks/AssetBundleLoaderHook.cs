/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Resources;
using CupheadArchipelago.Util;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks {
    internal class AssetBundleLoaderHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(loadAssetBundle));
            //Harmony.CreateAndPatchAll(typeof(loadAssetBundleSynchronous));
        }

        [HarmonyPatch(typeof(AssetBundleLoader), "loadAssetBundle", MethodType.Enumerator)]
        internal static class loadAssetBundle {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                int successTgt = 3;
                bool debug = false;

                MethodInfo iemethod = typeof(AssetBundleLoader).GetMethod("loadAssetBundle", BindingFlags.NonPublic | BindingFlags.Instance);
                Type ietype = Reflection.GetEnumeratorType(iemethod);
                FieldInfo _fi_e_assetBundleName = ietype.GetField("assetBundleName", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_e_request2 = ietype.GetField("<request>__2", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_getBasePath = typeof(AssetBundleLoader).GetMethod("getBasePath", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_LoadFromFileAsync = typeof(AssetBundle).GetMethod(
                    "LoadFromFileAsync",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(string)],
                    null
                );
                MethodInfo _mi_IsResourceAsset = typeof(AssetBundleLoaderHook).GetMethod("IsResourceAsset", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_LoadAssetBundleFromResourceAsync = typeof(loadAssetBundle).GetMethod("LoadAssetBundleFromResourceAsync", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_rjump = il.DefineLabel();
                Label l_vanillarq = il.DefineLabel();
                Label l_rq_stloc = il.DefineLabel();
            
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 4; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].opcode == OpCodes.Ldfld &&
                        codes[i + 3].opcode == OpCodes.Call && (MethodInfo)codes[i + 3].operand == _mi_getBasePath && codes[i + 4].opcode == OpCodes.Stfld
                    ) {
                        CodeInstruction[] ncodes = [
                            new CodeInstruction(OpCodes.Ldfld, _fi_e_assetBundleName),
                            new CodeInstruction(OpCodes.Call, _mi_IsResourceAsset),
                            new CodeInstruction(OpCodes.Brtrue, l_rjump),
                        ];
                        codes.InsertRange(i, ncodes);
                        i += ncodes.Length + 4;
                        success |= 1;
                    }
                    if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].opcode == OpCodes.Ldfld &&
                        codes[i + 3].opcode == OpCodes.Call && (MethodInfo)codes[i + 3].operand == _mi_LoadFromFileAsync && codes[i + 4].opcode == OpCodes.Stfld &&
                        (FieldInfo)codes[i + 4].operand == _fi_e_request2
                    ) {
                        codes[i].labels.Add(l_rjump);
                        codes[i + 2].labels.Add(l_vanillarq);
                        codes[i + 4].labels.Add(l_rq_stloc);
                        CodeInstruction[] ncodes = [
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, _fi_e_assetBundleName),
                            new CodeInstruction(OpCodes.Call, _mi_IsResourceAsset),
                            new CodeInstruction(OpCodes.Brfalse, l_vanillarq),
                            new CodeInstruction(OpCodes.Ldfld, _fi_e_assetBundleName),
                            new CodeInstruction(OpCodes.Call, _mi_LoadAssetBundleFromResourceAsync),
                            new CodeInstruction(OpCodes.Br, l_rq_stloc),
                        ];
                        codes.InsertRange(i + 2, ncodes);
                        i += ncodes.Length + 4;
                        success |= 2;
                    }
                    if (success == successTgt) break;
                }
                if (success == successTgt) throw new Exception($"{nameof(loadAssetBundle)}: Patch Failed! {success}");
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
                MethodInfo _mi_IsResourceAsset = typeof(AssetBundleLoaderHook).GetMethod("IsResourceAsset", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_LoadAssetBundleFromResource = typeof(loadAssetBundleSynchronous).GetMethod("LoadAssetBundleFromResource", BindingFlags.NonPublic | BindingFlags.Static);

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
                        break;
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

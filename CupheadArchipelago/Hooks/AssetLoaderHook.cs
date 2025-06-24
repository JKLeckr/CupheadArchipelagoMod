/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Util;
using HarmonyLib;
using UnityEngine;
using UnityEngine.U2D;

namespace CupheadArchipelago.Hooks {
    internal class AssetLoaderHook {
        private static readonly Type[] defaultGTypes = [typeof(SpriteAtlas), typeof(AudioClip)];
        private delegate IEnumerable<CodeInstruction> TranspilerAction(IEnumerable<CodeInstruction> instructions, ILGenerator il);

        /* TODO: Make this work correctly to be able to manage persistent assets and audio.
           Currently this only works for Sprite Assets. Patching AudioClips causes a NullReferenceException. 
        */

        internal static void Hook() {
            //Hook_Start();
            //Hook_loadPersistentAssets();
        }

        private static void Hook_Start() {
            HookGeneric(
                "Start",
                BindingFlags.NonPublic | BindingFlags.Instance,
                defaultGTypes,
                typeof(Start<>)
            );
        }
        private static void Hook_loadPersistentAssets() {
            HookGeneric(
                "loadPersistentAssets",
                BindingFlags.NonPublic | BindingFlags.Instance,
                defaultGTypes,
                typeof(loadPersistentAssets<>),
                MethodType.Enumerator
            );
        }

        private static class Start<T> {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                Type pcType = typeof(Start<>).MakeGenericType(typeof(T));
                Type pType = typeof(AssetLoader<>).MakeGenericType(typeof(T));
                FieldInfo _fi_sceneAssetDatabase = pType.GetField("sceneAssetDatabase", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_ProcessPersistentAssets = pcType.GetMethod("ProcessPersistentAssets", BindingFlags.NonPublic | BindingFlags.Static);

                Logging.Log(GetProcessMode());

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                CodeInstruction[] ncodes = [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _fi_sceneAssetDatabase),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)GetProcessMode()),
                    new CodeInstruction(OpCodes.Call, _mi_ProcessPersistentAssets),
                ];
                codes.InsertRange(0, ncodes);
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private enum Mode : sbyte {
                None = 0,
                SpriteAtlas = 1,
                AudioClip = 2,
            }
            private static Mode GetProcessMode() {
                if (typeof(T) == typeof(SpriteAtlas)) {
                    return Mode.SpriteAtlas;
                }
                if (typeof(T) == typeof(AudioClip)) {
                    return Mode.AudioClip;
                }
                return Mode.None;
            }

            private static void ProcessPersistentAssets(RuntimeSceneAssetDatabase sceneAssetDatabase, sbyte mode) {
                Logging.Log(sceneAssetDatabase);
                Logging.Log(sceneAssetDatabase.INTERNAL_persistentAssetNames);
                string[] pAssets = sceneAssetDatabase.INTERNAL_persistentAssetNames;
                Logging.Log($"PersistentAssets: {Aux.CollectionToString(pAssets)}");
            }
        }

        private static class loadPersistentAssets<T> {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                Type pcType = typeof(loadPersistentAssets<>).MakeGenericType(typeof(T));
                Type pType = typeof(AssetLoader<>).MakeGenericType(typeof(T));
                MethodInfo crmethod = pType.GetMethod("loadPersistentAssets", BindingFlags.NonPublic | BindingFlags.Instance);
                Type crtype = Reflection.GetEnumeratorType(crmethod);
                FieldInfo _fi_this = crtype.GetField("$this", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_locvar0 = crtype.GetField("$locvar0", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_sceneAssetDatabase = pType.GetField("sceneAssetDatabase", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_persistentAssets =
                    typeof(RuntimeSceneAssetDatabase).GetProperty("persistentAssets", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_LogPersistentAssets = pcType.GetMethod("LogPersistentAssets", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 6; i++) {
                    if (Dbg.C(codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldarg_0) && Dbg.C(codes[i + 2].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i + 2].operand == _fi_this) && Dbg.C(codes[i + 3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 3].operand == _fi_sceneAssetDatabase) &&
                        Dbg.C(codes[i + 4].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 4].operand == _mi_get_persistentAssets) && Dbg.C(codes[i + 5].opcode == OpCodes.Callvirt) &&
                        Dbg.C(codes[i + 6].opcode == OpCodes.Stfld && (FieldInfo)codes[i + 6].operand == _fi_locvar0)
                    ) {
                        codes.Insert(i + 5, new CodeInstruction(OpCodes.Call, _mi_LogPersistentAssets));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Transpiler)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static HashSet<string> LogPersistentAssets(HashSet<string> pAssets) {
                Logging.Log($"PersistentAssets: {Aux.CollectionToString(pAssets)}");
                return pAssets;
            }
        }
        
        private static void HookGeneric(
            string methodName,
            BindingFlags bindingFlags,
            Type[] gTypes,
            Type patchClass,
            MethodType methodType = MethodType.Normal
        ) {
            Harmony harmony = new($"CupheadArchipelago-AssetLoaderHook-generic-{Guid.NewGuid()}");

            foreach (Type gType in gTypes) {
                Type cType = typeof(AssetLoader<>).MakeGenericType(gType);
                MethodInfo targetM;
                switch (methodType) {
                    case MethodType.Normal: {
                            targetM = cType.GetMethod(methodName, bindingFlags);
                            break;
                        }
                    // Currently does not work correctly (locks game)
                    case MethodType.Enumerator: {
                            MethodInfo iemethod = cType.GetMethod(methodName, bindingFlags);
                            Type ietype = Reflection.GetEnumeratorType(iemethod);
                            targetM = ietype.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
                            break;
                        }

                    default:
                        throw new NotImplementedException($"{methodType} type not supported.");
                };

                Type tType = patchClass.MakeGenericType(gType);
                MethodInfo transpiler = tType.GetMethod("Transpiler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                harmony.Patch(targetM, transpiler: new HarmonyMethod(transpiler));
            }
        }
    }
}

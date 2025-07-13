/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Resources;
using CupheadArchipelago.Util;
using HarmonyLib;
using UnityEngine;
using UnityEngine.U2D;

namespace CupheadArchipelago.Hooks {
    internal class SceneLoaderHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(LoadScene));
            Harmony.CreateAndPatchAll(typeof(load_cr));
        }

        [HarmonyPatch(
            typeof(SceneLoader),
            "LoadScene",
            [
                typeof(Scenes),
                typeof(SceneLoader.Transition),
                typeof(SceneLoader.Transition),
                typeof(SceneLoader.Icon),
                typeof(SceneLoader.Context)
            ]
        )]
        internal static class LoadScene {
            static bool Prefix(Scenes scene) {
                if (scene == Scenes.scene_title) {
                    APClient.CloseArchipelagoSession();
                    DLCManagerHook.Reset();
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SceneLoader), "load_cr", MethodType.Enumerator)]
        internal static class load_cr {
            // Register assets that require DLC to function.
            private static readonly HashSet<string> dlcAssets = [
                "TitleCards_WDLC",
            ];

            // Asset Defs
            private static readonly HashSet<string> titleCardAssets = [
                "TitleCards_W1",
                "TitleCards_W2",
                "TitleCards_W3",
                "TitleCards_WDLC",
            ];

            // Scenes that require assets
            private static readonly Dictionary<string, HashSet<string>> sceneAddAtlases = new() {
                {Scenes.scene_map_world_1.ToString(), titleCardAssets},
                {Scenes.scene_map_world_2.ToString(), titleCardAssets},
                {Scenes.scene_map_world_3.ToString(), titleCardAssets},
                {Scenes.scene_map_world_DLC.ToString(), titleCardAssets},
            };
            private static readonly Dictionary<string, HashSet<string>> sceneAddMusic = new() { };

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                int successTgt = 15;
                bool debug = false;

                MethodInfo crmethod = typeof(SceneLoader).GetMethod("load_cr", BindingFlags.NonPublic | BindingFlags.Instance);
                Type crtype = Reflection.GetEnumeratorType(crmethod);
                FieldInfo _fi_cr_current = crtype.GetField("$current", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_cr_disposing = crtype.GetField("$disposing", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_cr_PC = crtype.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_cr_preloadAtlases = crtype.GetField("<preloadAtlases>__0", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_cr_preloadMusic = crtype.GetField("<preloadMusic>__0", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_previousSceneName = typeof(SceneLoader).GetField("previousSceneName", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_get_SceneName = typeof(SceneLoader).GetProperty("SceneName", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_GetPreloadAssetNames_SpriteAtlas =
                    typeof(AssetLoader<SpriteAtlas>).GetMethod("GetPreloadAssetNames", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_GetPreloadAssetNames_AudioClip =
                    typeof(AssetLoader<AudioClip>).GetMethod("GetPreloadAssetNames", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_UnloadResourceAssets = typeof(load_cr).GetMethod(nameof(UnloadResourceAssets), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_GetPreloadAtlases = typeof(load_cr).GetMethod(nameof(GetPreloadAtlases), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_GetPreloadMusic = typeof(load_cr).GetMethod(nameof(GetPreloadMusic), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_LoadResourceAssets = typeof(load_cr).GetMethod(nameof(LoadResourceAssets), BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 9; i++) {
                    if ((success & 1) == 0 && i < codes.Count - 11 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_SceneName &&
                        codes[i + 1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i + 1].operand == _fi_previousSceneName && codes[i + 2].opcode == OpCodes.Call &&
                        codes[i + 3].opcode == OpCodes.Brfalse && codes[i + 4].opcode == OpCodes.Call && (MethodInfo)codes[i + 4].operand == _mi_get_SceneName &&
                        codes[i + 5].opcode == OpCodes.Ldc_I4_2 && codes[i + 8].opcode == OpCodes.Constrained && (Type)codes[i + 8].operand == typeof(Scenes) &&
                        codes[i + 10].opcode == OpCodes.Call && codes[i + 11].opcode == OpCodes.Brfalse
                    ) {
                        codes.Insert(i + 12, new CodeInstruction(OpCodes.Call, _mi_UnloadResourceAssets));
                        i += 12;
                        success |= 1;
                    }
                    else if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_SpriteAtlas &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_cr_preloadAtlases
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadAtlases));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_get_SceneName));
                        i += 4;
                        success |= 2;
                    }
                    else if ((success & 4) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_AudioClip &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_cr_preloadMusic
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadMusic));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_get_SceneName));
                        i += 4;
                        success |= 4;
                    }
                    else if ((success & 8) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldnull && codes[i + 2].opcode == OpCodes.Stfld &&
                            (FieldInfo)codes[i + 2].operand == _fi_cr_current && codes[i + 3].opcode == OpCodes.Ldarg_0 && codes[i + 4].opcode == OpCodes.Ldfld &&
                            (FieldInfo)codes[i + 4].operand == _fi_cr_disposing && codes[i + 5].opcode == OpCodes.Brtrue && codes[i + 6].opcode == OpCodes.Ldarg_0 &&
                            codes[i + 7].opcode == OpCodes.Ldc_I4_5 && codes[i + 8].opcode == OpCodes.Stfld && (FieldInfo)codes[i + 8].operand == _fi_cr_PC &&
                            codes[i + 9].opcode == OpCodes.Br
                    ) {
                        codes[i + 1] = new CodeInstruction(OpCodes.Call, _mi_LoadResourceAssets);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_get_SceneName));
                        success |= 8;
                    }
                    if (success >= successTgt) break;
                }
                if (success != successTgt) throw new Exception($"{nameof(load_cr)}: Patch Failed! {success}");
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void UnloadResourceAssets() {
                Logging.LogDebug("Unloading resource assets...");
                AssetBundleMngr.UnloadAssetBundles();
                AssetMngr.UnloadAllAssets();
            }

            private static string[] GetPreloadAtlases(string sceneName, string[] preloadAtlases) {
                Logging.Log($"Scene name: {sceneName}");
                bool changed = false;
                HashSet<string> res = [.. preloadAtlases];
                // FIXME: Maybe not load ap stuff when in vanilla mode?
                if (sceneAddAtlases.ContainsKey(sceneName)) {
                    res.UnionWith(sceneAddAtlases[sceneName]);
                    if (!DLCManager.DLCEnabled()) res.ExceptWith(dlcAssets);
                    changed = true;
                }
                Dbg.LogCollectionDiff("Scene Atlases", preloadAtlases, changed ? res : null);
                return [.. res];
            }
            private static string[] GetPreloadMusic(string sceneName, string[] preloadMusic) {
                bool changed = false;
                HashSet<string> res = [.. preloadMusic];
                // FIXME: Maybe not load ap stuff when in vanilla mode?
                if (sceneAddMusic.ContainsKey(sceneName)) {
                    res.UnionWith(sceneAddMusic[sceneName]);
                    changed = true;
                }
                Dbg.LogCollectionDiff("Scene Audio", preloadMusic, changed ? res : null);
                return [.. res];
            }

            private static IEnumerator LoadResourceAssets(string sceneName) {
                if (SceneAssetMap.IsSceneRegistered(sceneName)) {
                    Logging.LogDebug("Loading resource assets...");
                    Dbg.LogCollection("Resource Assets", SceneAssetMap.GetSceneAssets(sceneName));
                    yield return AssetMngr.LoadSceneAssetsAsync(sceneName);
                }
                yield return null;
            }

            private static bool IsStringSceneName(string str, Scenes scene) {
                Scenes s = scene;
                return str == s.ToString();
            }
            private static bool IsAnyScene(string name, Scenes[] scenes) {
                foreach (Scenes scene in scenes) {
                    if (name == scene.ToString()) return true;
                }
                return false;
            }
        }
    }
}

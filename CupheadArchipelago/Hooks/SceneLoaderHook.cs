/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
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
            private static readonly HashSet<string> titleCardAssets = new() {
                {"TitleCards_W1"},
                {"TitleCards_W2"},
                {"TitleCards_W3"},
                {"TitleCards_WDLC"},
            };

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                MethodInfo crmethod = typeof(SceneLoader).GetMethod("load_cr", BindingFlags.NonPublic | BindingFlags.Instance);
                Type crtype = Reflection.GetEnumeratorType(crmethod);
                FieldInfo _fi_preloadAtlases = crtype.GetField("<preloadAtlases>__0", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _fi_preloadMusic = crtype.GetField("<preloadMusic>__0", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_SceneName = typeof(SceneLoader).GetProperty("SceneName", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_GetPreloadAssetNames_SpriteAtlas =
                    typeof(AssetLoader<SpriteAtlas>).GetMethod("GetPreloadAssetNames", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_GetPreloadAssetNames_AudioClip =
                    typeof(AssetLoader<AudioClip>).GetMethod("GetPreloadAssetNames", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_GetPreloadAtlases = typeof(load_cr).GetMethod(nameof(GetPreloadAtlases), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_GetPreloadMusic = typeof(load_cr).GetMethod(nameof(GetPreloadMusic), BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_SpriteAtlas &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_preloadAtlases
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadAtlases));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_get_SceneName));
                        i += 4;
                        success |= 1;
                    }
                    else if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_AudioClip &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_preloadMusic
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadMusic));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_get_SceneName));
                        i += 4;
                        success |= 2;
                    }
                    if (success >= 3) break;
                }
                if (success != 3) throw new Exception($"{nameof(load_cr)}: Patch Failed! {success}");
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static string[] GetPreloadAtlases(string sceneName, string[] preloadAtlases) {
                Logging.Log($"Scene name: {sceneName}");
                bool changed = false;
                HashSet<string> res = [.. preloadAtlases];
                if (PlayerData.inGame /*&& APData.IsCurrentSlotEnabled()*/) {
                    if (IsAnyScene(sceneName, [
                        Scenes.scene_map_world_1, Scenes.scene_map_world_2, Scenes.scene_map_world_3, Scenes.scene_map_world_DLC
                    ])) {
                        res.UnionWith(titleCardAssets);
                        changed = true;
                    }
                }
                Dbg.LogCollectionDiff("Scene Atlases", preloadAtlases, changed ? res : null);
                return [.. res];
            }
            private static string[] GetPreloadMusic(string sceneName, string[] preloadMusic) {
                bool changed = false;
                HashSet<string> res = [.. preloadMusic];
                Dbg.LogCollectionDiff("Scene Audio", preloadMusic, changed ? res : null);
                return [.. res];
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

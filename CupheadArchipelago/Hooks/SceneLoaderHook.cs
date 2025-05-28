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
            private static readonly Dictionary<string, string> assetBundles = new() {
                {"TitleCards_W1", "atlas_titlecards_w1"},
                {"TitleCards_W2", "atlas_titlecards_w2"},
                {"TitleCards_W3", "atlas_titlecards_w3"},
                {"TitleCards_WDLC", "atlas_titlecards_wdlc"},
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
                for (int i = 0; i < codes.Count - 7; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_SpriteAtlas &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_preloadAtlases
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadAtlases));
                        i += 3;
                        success |= 1;
                    }
                    else if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_SceneName &&
                       codes[i + 2].opcode == OpCodes.Call && (MethodInfo)codes[i + 2].operand == _mi_GetPreloadAssetNames_AudioClip &&
                       codes[i + 3].opcode == OpCodes.Stfld && codes[i + 3].operand == _fi_preloadMusic
                    ) {
                        codes.Insert(i + 3, new(OpCodes.Call, _mi_GetPreloadMusic));
                        i += 3;
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

            private static string[] GetPreloadAtlases(string[] preloadAtlases) {
                Logging.Log($"Atlases: {Aux.CollectionToString(preloadAtlases)}");
                return preloadAtlases;
            }
            private static string[] GetPreloadMusic(string[] preloadMusic) {
                Logging.Log($"Sounds: {Aux.CollectionToString(preloadMusic)}");
                return preloadMusic;
            }
        }
    }
}

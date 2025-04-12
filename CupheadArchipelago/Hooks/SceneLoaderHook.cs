/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class SceneLoaderHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(LoadScene));
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
    }
}

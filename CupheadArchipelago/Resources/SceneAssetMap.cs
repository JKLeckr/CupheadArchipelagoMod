/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class SceneAssetMap {
        private static readonly Dictionary<string, HashSet<string>> sceneAssetMap = new() {
            { Scenes.scene_slot_select.ToString(), ["testee"] },
            { Scenes.scene_level_dice_gate.ToString(), ["cap_dicehouse_chalkboard", "cap_dicehouse_chalkboard_spr"]},
        };

        public static bool IsSceneRegistered(Scenes scene) => IsSceneRegistered(scene.ToString());
        public static bool IsSceneRegistered(string sceneName) => sceneAssetMap.ContainsKey(sceneName);
        public static IEnumerable<string> GetRegisteredScenes() => sceneAssetMap.Keys;
        public static IEnumerable<string> GetSceneAssets(Scenes scene) => GetSceneAssets(scene.ToString());
        public static IEnumerable<string> GetSceneAssets(string sceneName) {
            if (sceneAssetMap.ContainsKey(sceneName))
                return sceneAssetMap[sceneName];
            else
                return [];
        }
    }
}

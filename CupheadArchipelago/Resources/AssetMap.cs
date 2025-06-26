/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class AssetMap {
        private static readonly Dictionary<string, HashSet<string>> assetMap = new() {
            {Scenes.scene_level_dice_gate.ToString(), ["cap_dicehouse_chalkboard", "cap_dicehouse_chalkboard_spr"]},
        };

        public static bool IsSceneRegistered(Scenes scene) => IsSceneRegistered(scene.ToString());
        public static bool IsSceneRegistered(string sceneName) => assetMap.ContainsKey(sceneName);
        public static IEnumerable<string> GetRegisteredScenes() => assetMap.Keys;
        public static IEnumerable<string> GetSceneAssets(Scenes scene) => GetSceneAssets(scene.ToString());
        public static IEnumerable<string> GetSceneAssets(string sceneName) {
            if (assetMap.ContainsKey(sceneName))
                return assetMap[sceneName];
            else
                return [];
        }
    }
}

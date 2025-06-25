/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    internal class AssetMap {
        private static readonly Dictionary<string, HashSet<string>> assetMap = new() {
            {Scenes.scene_level_dice_gate.ToString(), ["cap_dicehouse_chalkboard"]},
        };

        private static IEnumerable<string> GetSceneAssets(Scenes scene) => GetSceneAssets(scene.ToString());
        private static IEnumerable<string> GetSceneAssets(string sceneName) {
            if (assetMap.ContainsKey(sceneName))
                return assetMap[sceneName];
            else
                return [];
        }
    }
}

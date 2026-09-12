/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Mapping.Bits {
    public class MapBits {
        private static readonly Dictionary<Scenes, int> mapBits = new() {
            {Scenes.scene_map_world_1, 1 << 0},
            {Scenes.scene_map_world_2, 1 << 1},
            {Scenes.scene_map_world_3, 1 << 2},
            {Scenes.scene_map_world_4, 1 << 3},
            {Scenes.scene_map_world_DLC, 1 << 4}
        };

        public static int GetBit(Scenes map) {
            if (!mapBits.ContainsKey(map)) {
                throw new KeyNotFoundException("'" + map + "'" + " is not a valid map.");
            }
            return mapBits[map];
        }

        public static Scenes[] GetBittableMaps() {
            return [.. mapBits.Keys];
        }
    }
}

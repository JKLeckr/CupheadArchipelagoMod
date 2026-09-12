/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace CupheadArchipelago.Mapping.Bits {
    public class MapBits {
        private static readonly Dictionary<Scenes, int> mapBitIds = new() {
            {Scenes.scene_map_world_1, 0},
            {Scenes.scene_map_world_2, 1},
            {Scenes.scene_map_world_3, 2},
            {Scenes.scene_map_world_4, 3},
            {Scenes.scene_map_world_DLC, 4}
        };

        public static bool IsBittableMap(Scenes map) {
            return mapBitIds.ContainsKey(map);
        }

        public static int GetBit(Scenes map) {
            if (!mapBitIds.ContainsKey(map)) {
                throw new KeyNotFoundException("'" + map + "'" + " is not a valid map.");
            }
            return 1 << mapBitIds[map];
        }

        public static int GetBitId(Scenes map) {
            if (!mapBitIds.ContainsKey(map)) {
                throw new KeyNotFoundException("'" + map + "'" + " is not a valid map.");
            }
            return 1 << mapBitIds[map];
        }

        public static Scenes[] GetBittableMaps() {
            return [.. mapBitIds.Keys];
        }
    }
}

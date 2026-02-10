/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class APItemGroups {
        public enum ItemGroup {
            None = 0,
            Coins = 1,
        }

        private static readonly Dictionary<ItemGroup, APItem[]> itemGroups = new() {
            {ItemGroup.Coins, [APItem.coin, APItem.coin2, APItem.coin3]}
        };

        public static APItem[] GetItems(ItemGroup group) {
            if (itemGroups.ContainsKey(group)) {
                return itemGroups[group];
            }
            return [];
        }
    }
}

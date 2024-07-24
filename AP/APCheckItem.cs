/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using Archipelago.MultiClient.Net.Enums;

namespace CupheadArchipelago.AP {
    public readonly struct APCheckItem {
        public APCheckItem(long id, string name, APPlayer player, ItemFlags flags = 0) {
            Id = id;
            Name = name;
            Player = player;
            Flags = flags;
        }

        public long Id { get; }
        public string Name { get; }
        public APPlayer Player { get; }
        public ItemFlags Flags { get; }
    }
}

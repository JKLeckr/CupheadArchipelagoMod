/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using Archipelago.MultiClient.Net.Enums;

namespace CupheadArchipelago.AP {
    public class APItemInfo(long id, string name, ItemFlags flags)
    {
        public long Id { get; private set; } = id;
        public string Name { get; private set; } = name;
        public ItemFlags Flags { get; private set; } = flags;
    }
}

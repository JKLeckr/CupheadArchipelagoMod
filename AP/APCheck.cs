/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public readonly struct APCheck {
        public APCheck(long id, string name, APCheckItem item) {
            Id = id;
            Name = name;
            Item = item;
        }
        
        public long Id { get; }
        public string Name { get; }
        public APCheckItem Item { get; }
    }
}
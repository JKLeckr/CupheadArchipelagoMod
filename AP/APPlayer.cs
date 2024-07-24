/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public readonly struct APPlayer {
        public APPlayer(int id, string name, string alias = "") {
            Id = id;
            Name = name;
            Alias = alias;
        }

        public int Id { get; }
        public string Name { get; }
        public string Alias { get; }
    }
}

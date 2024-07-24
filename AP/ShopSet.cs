/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public readonly struct ShopSet {
        public ShopSet(int weapons, int charms) {
            Weapons = weapons;
            Charms = charms;
        }

        public int Weapons { get; }
        public int Charms { get; }
    }
}
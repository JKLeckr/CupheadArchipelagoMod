/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.Interfaces;

namespace CupheadArchipelago.Tests.TestClasses {
    internal class TPlayerData {
        internal static TPlayerData Data { get; private set; }

        public readonly HashSet<int> weapons = [];
        public readonly HashSet<int> charms = [];
        public readonly HashSet<int> supers = [];

        public int coins = 0;

        public bool planeSecondaryNotify = false;

        public static void Init() => Data = new TPlayerData();
    }

    internal class TPlayerDataItfc : IPlayerDataItfc {
        public bool IsUnlocked(Weapon weapon) =>
            TPlayerData.Data.weapons.Contains((int)weapon);

        public bool IsUnlocked(Charm charm) =>
            TPlayerData.Data.charms.Contains((int)charm);
        public bool IsUnlocked(Super super) =>
            TPlayerData.Data.supers.Contains((int)super);

        public void Gift(Weapon weapon) {
            TPlayerData.Data.weapons.Add((int)weapon);
        }
        public void Gift(Charm charm) {
            TPlayerData.Data.charms.Add((int)charm);
        }
        public void Gift(Super super) {
            TPlayerData.Data.supers.Add((int)super);
        }

        public void AddCoins(int count) {
            TPlayerData.Data.coins += count;
        }

        public int GetCoins() => TPlayerData.Data.coins;

        public void DoPlaneSecondaryEquipTrigger() {
            TPlayerData.Data.planeSecondaryNotify = true;
        }
    }
}

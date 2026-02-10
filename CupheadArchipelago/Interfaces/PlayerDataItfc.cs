/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Interfaces {
    internal interface IPlayerDataItfc {
        bool IsUnlocked(Weapon weapon);
        bool IsUnlocked(Charm charm);
        bool IsUnlocked(Super super);

        void Gift(Weapon weapon);
        void Gift(Charm charm);
        void Gift(Super super);

        void AddCoins(int count);
        int GetCoins();

        void DoPlaneSecondaryEquipTrigger();
    }

    internal class PlayerDataItfc : IPlayerDataItfc {
        internal static PlayerDataItfc Default { get => _default; }
        private static readonly PlayerDataItfc _default = new();

        public bool IsUnlocked(Weapon weapon) =>
            PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, weapon);
        public bool IsUnlocked(Charm charm) =>
            PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, charm);
        public bool IsUnlocked(Super super) =>
            PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, super);

        public void Gift(Weapon weapon) {
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, weapon))
                PlayerData.Data.Gift(PlayerId.PlayerOne, weapon);
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, weapon))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, weapon);
        }
        public void Gift(Charm charm) {
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, charm))
                PlayerData.Data.Gift(PlayerId.PlayerOne, charm);
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, charm))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, charm);
        }
        public void Gift(Super super) {
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, super))
                PlayerData.Data.Gift(PlayerId.PlayerOne, super);
            if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, super))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, super);
        }

        public void AddCoins(int count) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerOne, count);
            PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, count);
        }

        public int GetCoins() => PlayerData.Data.GetCurrency(PlayerId.PlayerOne);

        public void DoPlaneSecondaryEquipTrigger() {
            PlayerData
                .Data
                .Loadouts
                .GetPlayerLoadout(PlayerId.PlayerOne)
                .HasEquippedSecondarySHMUPWeapon = true;
            PlayerData
                .Data
                .Loadouts
                .GetPlayerLoadout(PlayerId.PlayerTwo)
                .HasEquippedSecondarySHMUPWeapon = true;
        }
    }
}

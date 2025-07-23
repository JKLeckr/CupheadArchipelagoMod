/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Hooks.PlayerHooks;
using UnityEngine;
using static CupheadArchipelago.Hooks.PlayerHooks.PlayerStatsManagerHook;

namespace CupheadArchipelago.Unity {
    internal class PlayerStatsManagerInterface : MonoBehaviour {
        private static PlayerStatsManagerInterface current1 = null;
        private static PlayerStatsManagerInterface current2 = null;

        private bool initted = false;

        private PlayerStatsManager stats;
        private PlayersStatsBossesHub bstats;

        internal void Init(PlayerStatsManager instance) {
            stats = instance;
            PlayerId playerId = stats.basePlayer.id;
            bstats = Level.GetPlayerStats(playerId);
            if (playerId == PlayerId.PlayerTwo) {
                if (current2 != null) {
                    Logging.LogError("StatsManagerInterface Current2 Exists");
                }
                current2 = this;
            }
            else {
                if (current1 != null) {
                    Logging.LogError("StatsManagerInterface Current1 Exists");
                }
                current1 = this;
            }
            Logging.Log($"[StatsManagerInterface] Initialized for {playerId}");
            initted = true;
        }

        void OnDestroy() {
            Logging.Log($"[StatsManagerInterface] Destroyed");
            initted = false;
            if (ReferenceEquals(current1, this)) current1 = null;
            if (ReferenceEquals(current2, this)) current2 = null;
        }

        public bool IsInitted() => initted;

        internal static PlayerStatsManagerInterface GetInstance(PlayerId playerId) {
            return playerId == PlayerId.PlayerTwo ? current2 : current1;
        }

        internal void AddEx(float add) {
            SetSuper(stats.SuperMeter + add);
        }
        internal void FillSuper() {
            SetSuper(DEFAULT_SUPER_FILL_AMOUNT);
        }
        internal void SetSuper(float set) {
            SetSuper(set, true);
        }
        private void SetSuper(float set, bool checkCanGainSuper) {
            if (stats.CanGainSuperMeter || !checkCanGainSuper) {
                PlayerStatsManagerHook.SetSuper(stats, set);
            }
        }

        internal bool IsDead() => stats.basePlayer.IsDead;

        internal int GetHealth() => stats.Health;
        internal void AddHealth(int add) {
            SetHealth(stats.Health + add);
        }
        internal void SetHealth(int set) {
            if (Level.IsInBossesHub) {
                bstats.BonusHP++;
            }
            if (!IsDead())
                stats.SetHealth(set);
            // TODO: Add a way to revive players without issues.
        }

        public static void AddEx(PlayerId playerId, float add) {
            Logging.Log($"Adding Ex for {playerId}");
            PlayerStatsManagerInterface instance = GetInstance(playerId);
            instance?.SetSuper(instance.stats.SuperMeter + add);
        }
        public static void FillSuper(PlayerId playerId) {
            Logging.Log($"Filling Super for {playerId}");
            SetSuper(playerId, DEFAULT_SUPER_FILL_AMOUNT);
        }
        public static void SetSuper(PlayerId playerId, float set) {
            Logging.Log($"Setting Super for {playerId}");
            GetInstance(playerId)?.SetSuper(set);
        }

        public static int GetHealth(PlayerId playerId) {
            return GetInstance(playerId)?.GetHealth() ?? 0;
        }
        public static void AddHealth(PlayerId playerId, int add) {
            Logging.Log($"Adding Health for {playerId}");
            GetInstance(playerId)?.AddHealth(add);
        }
        public static void SetHealth(PlayerId playerId, int set) {
            Logging.Log($"Setting Health for {playerId}");
            GetInstance(playerId)?.SetHealth(set);
        }

        public void KillPlayer() {
            IssueStatsCommand(stats, StatsCommands.Death);
        }
        public static void KillPlayer(PlayerId playerId) {
            if (playerId == PlayerId.PlayerOne || playerId == PlayerId.Any)
                GetInstance(PlayerId.PlayerOne)?.KillPlayer();
            if (playerId == PlayerId.PlayerTwo || playerId == PlayerId.Any)
                GetInstance(PlayerId.PlayerTwo)?.KillPlayer();
        }

        internal void ReverseControls() {
            IssueStatsCommand(stats, StatsCommands.ReverseControls);
        }
        internal static void ReverseControls(PlayerId playerId) {
            GetInstance(playerId)?.ReverseControls();
        }
    }
}

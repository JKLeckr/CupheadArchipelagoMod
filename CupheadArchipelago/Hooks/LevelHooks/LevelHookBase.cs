/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class LevelHookBase {
        internal static bool IsChalice() {
            return PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).charm == Charm.charm_chalice || (
                PlayerManager.Multiplayer && PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).charm == Charm.charm_chalice
            );
        }
    }
}

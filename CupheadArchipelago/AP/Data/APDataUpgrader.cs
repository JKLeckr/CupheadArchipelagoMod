// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using CupheadArchipelago.Data;
using CupheadArchipelago.Mapping.Bits;

namespace CupheadArchipelago.AP.Data {
    internal class APDataUpgrader {
        internal static bool UpgradeStatsV4to5(int slot) {
            APData data = APData.SData[slot];

            if (data.version != 4) {
                return false;
            }

            Logging.Log("[APData] Save data is an older version (4). Upgrading to version 5.");

            PlayerData pdata = PlayerData.GetDataForSlot(slot);

            data.playerData.agrade_levels = 0;
            data.playerData.pacifist_levels = 0;
            data.playerData.dlc_chaliced_levels_p1 = 0;
            data.playerData.dlc_chaliced_levels_p2 = 0;

            foreach (Levels level in Level.chaliceLevels) {
                PlayerData.PlayerLevelDataObject ldata = pdata.GetLevelData(level);
                if (ldata.grade >= LevelScoringData.Grade.AMinus) {
                    APClient.APSessionGSPlayerData.agrade_levels |= LevelBits.GetLevelBit(level);
                }
                if (ldata.completedAsChaliceP1) {
                    APClient.APSessionGSPlayerData.dlc_chaliced_levels_p1 |= LevelBits.GetLevelBit(level);
                }
                if (ldata.completedAsChaliceP2) {
                    APClient.APSessionGSPlayerData.dlc_chaliced_levels_p2 |= LevelBits.GetLevelBit(level);
                }
            }

            foreach (Levels level in Level.platformingLevels) {
                PlayerData.PlayerLevelDataObject ldata = pdata.GetLevelData(level);
                if (ldata.grade >= LevelScoringData.Grade.AMinus) {
                    APClient.APSessionGSPlayerData.agrade_levels |= LevelBits.GetLevelBit(level);
                }
                if (ldata.grade == LevelScoringData.Grade.P) {
                    APClient.APSessionGSPlayerData.pacifist_levels |= LevelBits.GetLevelBit(level);
                }
                if (ldata.completedAsChaliceP1) {
                    APClient.APSessionGSPlayerData.dlc_chaliced_levels_p1 |= LevelBits.GetLevelBit(level);
                }
                if (ldata.completedAsChaliceP2) {
                    APClient.APSessionGSPlayerData.dlc_chaliced_levels_p2 |= LevelBits.GetLevelBit(level);
                }
            }

            return true;
        }
    }
}

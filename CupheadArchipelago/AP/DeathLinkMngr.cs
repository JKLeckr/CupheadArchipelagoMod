/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    internal class DeathLinkMngr {
        public static void TriggerSendDeathLink(Level instance) {
            Logging.Log("DeathLink");
            if (APManager.Current != null && !APManager.Current.IsDeathTriggered()) {
                if (instance.LevelType == Level.Type.Platforming) {
                    APClient.SendDeath(LevelNames.platformLevelNames[instance.CurrentLevel], DeathLinkCauseType.Normal);
                }
                else if (instance.LevelType == Level.Type.Tutorial) {
                    APClient.SendDeath(instance.CurrentLevel.ToString(), DeathLinkCauseType.Tutorial);
                }
                else {
                    if (instance.CurrentLevel == Levels.Mausoleum) {
                        APClient.SendDeath("Mausoleum", DeathLinkCauseType.Mausoleum);
                    }
                    else if (Array.Exists(Level.kingOfGamesLevels, (Levels level) => instance.CurrentLevel == level)) {
                        APClient.SendDeath(LevelNames.bossNames[instance.CurrentLevel], DeathLinkCauseType.ChessCastle);
                    }
                    else if (instance.CurrentLevel == Levels.Graveyard) {
                        APClient.SendDeath(LevelNames.bossNames[Levels.Graveyard], DeathLinkCauseType.Graveyard);
                    }
                    else {
                        APClient.SendDeath(LevelNames.bossNames[instance.CurrentLevel], DeathLinkCauseType.Boss);
                    }
                }
            }
        }
    }
}

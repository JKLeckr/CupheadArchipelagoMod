/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    internal class LevelNames {
        internal static readonly Dictionary<Levels, string> bossNames = new() {
            {Levels.Veggies, "The Root Pack"},
            {Levels.Slime, "Goopy Le Grande"},
            {Levels.FlyingBlimp, "Hilda Berg"},
            {Levels.Flower, "Cagney Carnation"},
            {Levels.Frogs, "Ribby and Croaks"},
            {Levels.Baroness, "Barnoness Von Bon Bon"},
            {Levels.Clown, "Beppi the Clown"},
            {Levels.FlyingGenie, "Djimmi the Great"},
            {Levels.Dragon, "Grim Matchstick"},
            {Levels.FlyingBird, "Wally Warbles"},
            {Levels.Bee, "Rumor Honeybottoms"},
            {Levels.Pirate, "Captain Brineybeard"},
            {Levels.SallyStagePlay, "Sally Stageplay"},
            {Levels.Mouse, "Werner Werman"},
            {Levels.Robot, "Dr. Kahl's Robot"},
            {Levels.FlyingMermaid, "Cala Maria"},
            {Levels.Train, "The Phanton Express"},
            {Levels.DicePalaceBooze, "The Tipsy Troop"},
            {Levels.DicePalaceChips, "Chips Bettigan"},
            {Levels.DicePalaceCigar, "Mr. Wheezy"},
            {Levels.DicePalaceDomino, "Pip and Dot"},
            {Levels.DicePalaceRabbit, "Hopus Pocus"},
            {Levels.DicePalaceFlyingHorse, "Phear Lap"},
            {Levels.DicePalaceRoulette, "Pirouletta"},
            {Levels.DicePalaceEightBall, "Mangosteen"},
            {Levels.DicePalaceFlyingMemory, "Mr. Chimes"},
            {Levels.DicePalaceMain, "King Dice"},
            {Levels.Devil, "The Devil"},
            {Levels.OldMan, "Glumstone the Giant"},
            {Levels.SnowCult, "Mortimer Freeze"},
            {Levels.RumRunners, "The Moonshine Mob"},
            {Levels.FlyingCowboy, "Esther Winchester"},
            {Levels.Airplane, "The Howling Aces"},
            {Levels.Saltbaker, "Chef Saltbaker"},
            {Levels.Graveyard, "The Angel and The Demon"},
            {Levels.ChessPawn, "The Pawns"},
            {Levels.ChessKnight, "The Knight"},
            {Levels.ChessBishop, "The Bishop"},
            {Levels.ChessRook, "The Rook"},
            {Levels.ChessQueen, "The Queen"},
        };
        internal static readonly Dictionary<Levels, string> platformLevelNames = new() {
            {Levels.Platforming_Level_1_1, "Forest Follies"},
            {Levels.Platforming_Level_1_2, "Treetop Trouble"},
            {Levels.Platforming_Level_2_1, "Funhouse Frazzle"},
            {Levels.Platforming_Level_2_2, "Funfair Fever"},
            {Levels.Platforming_Level_3_1, "Perilous Piers"},
            {Levels.Platforming_Level_3_2, "Rugged Ridge"},
        };
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class LevelLocationMap {
        private static readonly Dictionary<Levels,APLocation[]> map;

        static LevelLocationMap() {
            map = new Dictionary<Levels, APLocation[]>() {
                {Levels.Veggies, new APLocation[] {
                    APLocation.level_boss_veggies,
                    APLocation.level_boss_veggies_topgrade,
                    APLocation.level_boss_veggies_dlc_chaliced,
                    APLocation.level_boss_veggies_secret
                }},
                {Levels.Slime, new APLocation[] {
                    APLocation.level_boss_slime,
                    APLocation.level_boss_slime_topgrade,
                    APLocation.level_boss_slime_dlc_chaliced
                }},
                {Levels.Frogs, new APLocation[] {
                    APLocation.level_boss_frogs,
                    APLocation.level_boss_frogs_topgrade,
                    APLocation.level_boss_frogs_dlc_chaliced
                }},
                {Levels.Flower, new APLocation[] {
                    APLocation.level_boss_flower,
                    APLocation.level_boss_flower_topgrade,
                    APLocation.level_boss_flower_dlc_chaliced
                }},
                {Levels.Baroness, new APLocation[] {
                    APLocation.level_boss_baroness,
                    APLocation.level_boss_baroness_topgrade,
                    APLocation.level_boss_baroness_dlc_chaliced
                }},
                {Levels.Clown, new APLocation[] {
                    APLocation.level_boss_clown,
                    APLocation.level_boss_clown_topgrade,
                    APLocation.level_boss_clown_dlc_chaliced
                }},
                {Levels.Dragon, new APLocation[] {
                    APLocation.level_boss_dragon,
                    APLocation.level_boss_dragon_topgrade,
                    APLocation.level_boss_dragon_dlc_chaliced
                }},
                {Levels.Bee, new APLocation[] {
                    APLocation.level_boss_bee,
                    APLocation.level_boss_bee_topgrade,
                    APLocation.level_boss_bee_dlc_chaliced
                }},
                {Levels.Pirate, new APLocation[] {
                    APLocation.level_boss_pirate,
                    APLocation.level_boss_pirate_topgrade,
                    APLocation.level_boss_pirate_dlc_chaliced
                }},
                {Levels.Mouse, new APLocation[] {
                    APLocation.level_boss_mouse,
                    APLocation.level_boss_mouse_topgrade,
                    APLocation.level_boss_mouse_dlc_chaliced
                }},
                {Levels.SallyStagePlay, new APLocation[] {
                    APLocation.level_boss_sallystageplay,
                    APLocation.level_boss_sallystageplay_topgrade,
                    APLocation.level_boss_sallystageplay_dlc_chaliced,
                    APLocation.level_boss_sallystageplay_secret
                }},
                {Levels.Train, new APLocation[] {
                    APLocation.level_boss_train,
                    APLocation.level_boss_train_topgrade,
                    APLocation.level_boss_train_dlc_chaliced
                }},

                {Levels.DicePalaceMain, new APLocation[] {
                    APLocation.level_boss_kingdice,
                    APLocation.level_boss_kingdice_topgrade,
                    APLocation.level_boss_kingdice_dlc_chaliced
                }},

                {Levels.DicePalaceBooze, new APLocation[] {
                    APLocation.level_dicepalace_boss_booze,
                }},
                {Levels.DicePalaceChips, new APLocation[] {
                    APLocation.level_dicepalace_boss_chips,
                }},
                {Levels.DicePalaceCigar, new APLocation[] {
                    APLocation.level_dicepalace_boss_cigar,
                }},
                {Levels.DicePalaceDomino, new APLocation[] {
                    APLocation.level_dicepalace_boss_domino,
                }},
                {Levels.DicePalaceRabbit, new APLocation[] {
                    APLocation.level_dicepalace_boss_rabbit,
                }},
                {Levels.DicePalaceFlyingHorse, new APLocation[] {
                    APLocation.level_dicepalace_boss_plane_horse,
                }},
                {Levels.DicePalaceRoulette, new APLocation[] {
                    APLocation.level_dicepalace_boss_roulette,
                }},
                {Levels.DicePalaceEightBall, new APLocation[] {
                    APLocation.level_dicepalace_boss_eightball,
                }},
                {Levels.DicePalaceFlyingMemory, new APLocation[] {
                    APLocation.level_dicepalace_boss_plane_memory,
                }},

                {Levels.Devil, new APLocation[] {
                    APLocation.level_boss_devil,
                    APLocation.level_boss_devil_topgrade,
                    APLocation.level_boss_devil_dlc_chaliced
                }},

                {Levels.FlyingBlimp, new APLocation[] {
                    APLocation.level_boss_plane_blimp,
                    APLocation.level_boss_plane_blimp_topgrade,
                    APLocation.level_boss_plane_blimp_dlc_chaliced
                }},
                {Levels.FlyingGenie, new APLocation[] {
                    APLocation.level_boss_plane_genie,
                    APLocation.level_boss_plane_genie_topgrade,
                    APLocation.level_boss_plane_genie_dlc_chaliced,
                    APLocation.level_boss_plane_genie_secret
                }},
                {Levels.FlyingBird, new APLocation[] {
                    APLocation.level_boss_plane_bird,
                    APLocation.level_boss_plane_bird_topgrade,
                    APLocation.level_boss_plane_bird_dlc_chaliced
                }},
                {Levels.FlyingMermaid, new APLocation[] {
                    APLocation.level_boss_plane_mermaid,
                    APLocation.level_boss_plane_mermaid_topgrade,
                    APLocation.level_boss_plane_mermaid_dlc_chaliced
                }},
                {Levels.Robot, new APLocation[] {
                    APLocation.level_boss_plane_robot,
                    APLocation.level_boss_plane_robot_topgrade,
                    APLocation.level_boss_plane_robot_dlc_chaliced
                }},

                {Levels.Platforming_Level_1_1, new APLocation[] {
                    APLocation.level_rungun_forest,
                    APLocation.level_rungun_forest_agrade,
                    APLocation.level_rungun_forest_pacifist,
                    APLocation.level_rungun_forest_dlc_chaliced,
                }},
                {Levels.Platforming_Level_1_2, new APLocation[] {
                    APLocation.level_rungun_tree,
                    APLocation.level_rungun_tree_agrade,
                    APLocation.level_rungun_tree_pacifist,
                    APLocation.level_rungun_tree_dlc_chaliced,
                }},
                {Levels.Platforming_Level_2_1, new APLocation[] {
                    APLocation.level_rungun_circus,
                    APLocation.level_rungun_circus_agrade,
                    APLocation.level_rungun_circus_pacifist,
                    APLocation.level_rungun_circus_dlc_chaliced,
                }},
                {Levels.Platforming_Level_2_2, new APLocation[] {
                    APLocation.level_rungun_funhouse,
                    APLocation.level_rungun_funhouse_agrade,
                    APLocation.level_rungun_funhouse_pacifist,
                    APLocation.level_rungun_funhouse_dlc_chaliced,
                }},
                {Levels.Platforming_Level_3_1, new APLocation[] {
                    APLocation.level_rungun_harbour,
                    APLocation.level_rungun_harbour_agrade,
                    APLocation.level_rungun_harbour_pacifist,
                    APLocation.level_rungun_harbour_dlc_chaliced,
                }},
                {Levels.Platforming_Level_3_2, new APLocation[] {
                    APLocation.level_rungun_mountain,
                    APLocation.level_rungun_mountain_agrade,
                    APLocation.level_rungun_mountain_pacifist,
                    APLocation.level_rungun_mountain_dlc_chaliced,
                }},

                {Levels.OldMan, new APLocation[] {
                    APLocation.level_dlc_boss_oldman,
                    APLocation.level_dlc_boss_oldman_topgrade,
                    APLocation.level_dlc_boss_oldman_dlc_chaliced
                }},
                {Levels.RumRunners, new APLocation[] {
                    APLocation.level_dlc_boss_rumrunners,
                    APLocation.level_dlc_boss_rumrunners_topgrade,
                    APLocation.level_dlc_boss_rumrunners_dlc_chaliced
                }},
                {Levels.SnowCult, new APLocation[] {
                    APLocation.level_dlc_boss_snowcult,
                    APLocation.level_dlc_boss_snowcult_topgrade,
                    APLocation.level_dlc_boss_snowcult_dlc_chaliced
                }},
                {Levels.Airplane, new APLocation[] {
                    APLocation.level_dlc_boss_airplane,
                    APLocation.level_dlc_boss_airplane_topgrade,
                    APLocation.level_dlc_boss_airplane_dlc_chaliced
                }},
                {Levels.FlyingCowboy, new APLocation[] {
                    APLocation.level_dlc_boss_plane_cowboy,
                    APLocation.level_dlc_boss_plane_cowboy_topgrade,
                    APLocation.level_dlc_boss_plane_cowboy_dlc_chaliced
                }},

                {Levels.Saltbaker, new APLocation[] {
                    APLocation.level_dlc_boss_saltbaker,
                    APLocation.level_dlc_boss_saltbaker_topgrade,
                    APLocation.level_dlc_boss_saltbaker_dlc_chaliced
                }},

                {Levels.ChessPawn, new APLocation[] {
                    APLocation.level_dlc_chesscastle_pawn,
                }},
                {Levels.ChessKnight, new APLocation[] {
                    APLocation.level_dlc_chesscastle_knight,
                }},
                {Levels.ChessBishop, new APLocation[] {
                    APLocation.level_dlc_chesscastle_bishop,
                }},
                {Levels.ChessRook, new APLocation[] {
                    APLocation.level_dlc_chesscastle_rook,
                }},
                {Levels.ChessQueen, new APLocation[] {
                    APLocation.level_dlc_chesscastle_queen,
                }},
            };
        }

        public static long GetLocationId(Levels level, int index) => map[level][index].id;
        public static IEnumerable<Levels> GetKeys() => map.Keys;
    }
}
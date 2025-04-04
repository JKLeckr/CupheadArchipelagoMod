/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
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
                    new(-254), //TEMP
                }},
                {Levels.DicePalaceChips, new APLocation[] {
                    APLocation.level_dicepalace_boss_chips,
                    new(-253), //TEMP
                }},
                {Levels.DicePalaceCigar, new APLocation[] {
                    APLocation.level_dicepalace_boss_cigar,
                    new(-252), //TEMP
                }},
                {Levels.DicePalaceDomino, new APLocation[] {
                    APLocation.level_dicepalace_boss_domino,
                    new(-251), //TEMP
                }},
                {Levels.DicePalaceRabbit, new APLocation[] {
                    APLocation.level_dicepalace_boss_rabbit,
                    new(-250), //TEMP
                }},
                {Levels.DicePalaceFlyingHorse, new APLocation[] {
                    APLocation.level_dicepalace_boss_plane_horse,
                    new(-249), //TEMP
                }},
                {Levels.DicePalaceRoulette, new APLocation[] {
                    APLocation.level_dicepalace_boss_roulette,
                    new(-248), //TEMP
                }},
                {Levels.DicePalaceEightBall, new APLocation[] {
                    APLocation.level_dicepalace_boss_eightball,
                    new(-247), //TEMP
                }},
                {Levels.DicePalaceFlyingMemory, new APLocation[] {
                    APLocation.level_dicepalace_boss_plane_memory,
                    new(-246), //TEMP
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
                    APLocation.level_rungun_forest_coin1,
                    APLocation.level_rungun_forest_coin2,
                    APLocation.level_rungun_forest_coin3,
                    APLocation.level_rungun_forest_coin4,
                    APLocation.level_rungun_forest_coin5
                }},
                {Levels.Platforming_Level_1_2, new APLocation[] {
                    APLocation.level_rungun_tree,
                    APLocation.level_rungun_tree_agrade,
                    APLocation.level_rungun_tree_pacifist,
                    APLocation.level_rungun_tree_dlc_chaliced,
                    APLocation.level_rungun_tree_coin1,
                    APLocation.level_rungun_tree_coin2,
                    APLocation.level_rungun_tree_coin3,
                    APLocation.level_rungun_tree_coin4,
                    APLocation.level_rungun_tree_coin5
                }},
                {Levels.Platforming_Level_2_1, new APLocation[] {
                    APLocation.level_rungun_circus,
                    APLocation.level_rungun_circus_agrade,
                    APLocation.level_rungun_circus_pacifist,
                    APLocation.level_rungun_circus_dlc_chaliced,
                    APLocation.level_rungun_circus_coin1,
                    APLocation.level_rungun_circus_coin2,
                    APLocation.level_rungun_circus_coin3,
                    APLocation.level_rungun_circus_coin4,
                    APLocation.level_rungun_circus_coin5
                }},
                {Levels.Platforming_Level_2_2, new APLocation[] {
                    APLocation.level_rungun_funhouse,
                    APLocation.level_rungun_funhouse_agrade,
                    APLocation.level_rungun_funhouse_pacifist,
                    APLocation.level_rungun_funhouse_dlc_chaliced,
                    APLocation.level_rungun_funhouse_coin1,
                    APLocation.level_rungun_funhouse_coin2,
                    APLocation.level_rungun_funhouse_coin3,
                    APLocation.level_rungun_funhouse_coin4,
                    APLocation.level_rungun_funhouse_coin5
                }},
                {Levels.Platforming_Level_3_1, new APLocation[] {
                    APLocation.level_rungun_harbour,
                    APLocation.level_rungun_harbour_agrade,
                    APLocation.level_rungun_harbour_pacifist,
                    APLocation.level_rungun_harbour_dlc_chaliced,
                    APLocation.level_rungun_harbour_coin1,
                    APLocation.level_rungun_harbour_coin2,
                    APLocation.level_rungun_harbour_coin3,
                    APLocation.level_rungun_harbour_coin4,
                    APLocation.level_rungun_harbour_coin5
                }},
                {Levels.Platforming_Level_3_2, new APLocation[] {
                    APLocation.level_rungun_mountain,
                    APLocation.level_rungun_mountain_agrade,
                    APLocation.level_rungun_mountain_pacifist,
                    APLocation.level_rungun_mountain_dlc_chaliced,
                    APLocation.level_rungun_mountain_coin1,
                    APLocation.level_rungun_mountain_coin2,
                    APLocation.level_rungun_mountain_coin3,
                    APLocation.level_rungun_mountain_coin4,
                    APLocation.level_rungun_mountain_coin5
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
                    new(-255), //TEMP
                }},
                {Levels.ChessKnight, new APLocation[] {
                    APLocation.level_dlc_chesscastle_knight,
                    new(-256), //TEMP
                }},
                {Levels.ChessBishop, new APLocation[] {
                    APLocation.level_dlc_chesscastle_bishop,
                    new(-257), //TEMP
                }},
                {Levels.ChessRook, new APLocation[] {
                    APLocation.level_dlc_chesscastle_rook,
                    new(-258), //TEMP
                }},
                {Levels.ChessQueen, new APLocation[] {
                    APLocation.level_dlc_chesscastle_queen,
                    new(-259), //TEMP
                }},
            };
        }

        public static long GetLocationId(Levels level, int index) {
            try {
                return map[level][index].id;
            } catch (KeyNotFoundException) {
                throw new KeyNotFoundException($"Level {level} does not exist.");
            } catch (IndexOutOfRangeException) {
                throw new IndexOutOfRangeException($"Index {index} is out of range for Level {level}.");
            }
        }
        public static bool LevelHasLocations(Levels level) => map.ContainsKey(level);
        public static IEnumerable<Levels> GetKeys() => map.Keys;
    }
}
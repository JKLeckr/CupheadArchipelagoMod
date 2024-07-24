/// Copyright 2024 JKLeckr
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
                    APLocation.level_boss_veggies_dlc_chaliced
                }},
                {Levels.Slime, new APLocation[] {
                    APLocation.level_boss_slime,
                    APLocation.level_boss_slime_topgrade,
                    APLocation.level_boss_slime_dlc_chaliced
                }},
                {Levels.Frogs, new APLocation[] {
                    APLocation.level_boss_frogs,
                    APLocation.level_boss_frogs_topgrade
                }},
                {Levels.Flower, new APLocation[] {
                    APLocation.level_boss_flower,
                    APLocation.level_boss_flower_topgrade
                }},
                {Levels.Baroness, new APLocation[] {
                    APLocation.level_boss_baroness,
                    APLocation.level_boss_baroness_topgrade
                }},
                {Levels.Clown, new APLocation[] {
                    APLocation.level_boss_clown,
                    APLocation.level_boss_clown_topgrade
                }},
                {Levels.Dragon, new APLocation[] {
                    APLocation.level_boss_dragon,
                    APLocation.level_boss_dragon_topgrade
                }},
                {Levels.Bee, new APLocation[] {
                    APLocation.level_boss_bee,
                    APLocation.level_boss_bee_topgrade
                }},
                {Levels.Pirate, new APLocation[] {
                    APLocation.level_boss_pirate,
                    APLocation.level_boss_pirate_topgrade
                }},
                {Levels.Mouse, new APLocation[] {
                    APLocation.level_boss_mouse,
                    APLocation.level_boss_mouse_topgrade
                }},
                {Levels.SallyStagePlay, new APLocation[] {
                    APLocation.level_boss_sallystageplay,
                    APLocation.level_boss_sallystageplay_topgrade
                }},
                {Levels.Train, new APLocation[] {
                    APLocation.level_boss_train,
                    APLocation.level_boss_train_topgrade
                }},
                {Levels.DicePalaceMain, new APLocation[] {
                    APLocation.level_boss_kingdice,
                    APLocation.level_boss_kingdice_topgrade
                }},
                {Levels.Devil, new APLocation[] {
                    APLocation.level_boss_devil,
                    APLocation.level_boss_devil_topgrade
                }},
            };
        }

        public static long GetLocationId(Levels level, int index) => map[level][index].Id;
        public static string GetLocationName(Levels level, int index) => map[level][index].Name;
        public static IEnumerable<Levels> GetKeys() => map.Keys;
    }
}
/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class APLevel {
        private static Dictionary<string, APLevel> map = new();
        private static Dictionary<Levels, string> level_to_name = new();
        public string Name { get; private set; }
        public Levels Level { get; private set; }

        public APLevel (string name, Levels level) {
            Name = name;
            Level = level;
            map.Add(name, this);
            level_to_name.Add(level, name);
        }

        public override string ToString() { return Name; }

        //TODO: Fill out later

        public static APLevel FromLevelId(Levels level) => FromName(LevelToName(level));
        //TODO: Implement in generator
        public static APLevel FromName(string name) => map[name];
        public static bool LevelExists(Levels level) => level_to_name.ContainsKey(level);
        public static bool NameExists(string name) => map.ContainsKey(name);
        public static string LevelToName(Levels level) => level_to_name[level];
        public static Levels NameToLevel(string name) => map[name].Level;
    }
}
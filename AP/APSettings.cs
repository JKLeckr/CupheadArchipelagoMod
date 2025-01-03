/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public class APSettings {
        public static bool UseDLC { get; internal set; }
        public static GameMode Mode { get; internal set; }
        public static bool Hard { get; internal set; }
        public static APItem StartWeapon { get; internal set; }
        public static bool FreemoveIsles { get; internal set; }
        public static bool RandomizeAbilities { get; internal set; }
        public static bool BossSecretChecks { get; internal set; }
        public static GradeChecks BossGradeChecks { get; internal set; }
        public static GradeChecks RungunGradeChecks { get; internal set; }
        public static bool DeathLink { get; internal set; }
        public static bool QuestJuggler { get; internal set; }
        public static bool QuestPacifist { get; internal set; }
        public static bool QuestProfessional { get; internal set; }
        public static int StartMaxHealth { get; internal set; }
        public static int[] RequiredContracts { get; internal set; }
        public static int RequiredIngredients { get; internal set; }
        public static bool AllowGameDjimmi { get; internal set; }
        public static bool ShowUnaccessibleIslesInList { get; internal set; }

        static APSettings() => Init();
        public static void Init() {
            UseDLC = false;
            Mode = GameMode.BeatDevil;
            Hard = false;
            StartWeapon = APItem.weapon_peashooter;
            FreemoveIsles = false;
            RandomizeAbilities = false;
            BossSecretChecks = false;
            BossGradeChecks = GradeChecks.Disabled;
            RungunGradeChecks = GradeChecks.Disabled;
            DeathLink = false;
            QuestJuggler = true;
            QuestPacifist = false;
            QuestProfessional = false;
            StartMaxHealth = 3;
            RequiredContracts = [5,10,17];
            RequiredIngredients = 5;
            AllowGameDjimmi = false;
            ShowUnaccessibleIslesInList = false;
        }
    }
}
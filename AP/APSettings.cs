/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public class APSettings {
        public static bool UseDLC { get; internal set; } = false;
        public static GameMode Mode { get; internal set; } = GameMode.BeatDevil;
        public static bool Hard { get; internal set; } = false;
        public static APItem StartWeapon { get; internal set; } = APItem.weapon_peashooter;
        public static bool FreemoveIsles { get; internal set; } = false;
        public static bool RandomizeAbilities { get; internal set; } = false;
        public static GradeChecks BossGradeChecks { get; internal set; } = GradeChecks.Disabled;
        public static GradeChecks RungunGradeChecks { get; internal set; } = GradeChecks.Disabled;
        public static bool DeathLink { get; internal set; } = false;
        public static bool QuestJuggler { get; internal set; } = true;
        public static bool QuestPacifist { get; internal set; } = false;
        public static bool QuestProfessional { get; internal set; } = false;
        public static int StartMaxHealth { get; internal set; } = 3;
        public static int[] RequiredContracts { get; internal set; } = [5,10,17];
        public static int RequiredIngredients { get; internal set; } = 5;
    }
}
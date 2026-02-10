/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.AP {
    public class APSettings {
        public static bool UseDLC { get; internal set; }
        public static GameModes Mode { get; internal set; }
        public static bool Hard { get; internal set; }
        public static APItem StartWeapon { get; internal set; }
        public static WeaponModes WeaponMode { get; internal set; }
        public static bool FreemoveIsles { get; internal set; }
        public static bool RandomizeAbilities { get; internal set; }
        public static bool BossSecretChecks { get; internal set; }
        public static GradeChecks BossGradeChecks { get; internal set; }
        public static GradeChecks RungunGradeChecks { get; internal set; }
        public static bool DicePalaceBossSanity { get; internal set; }
        public static DeathLinkMode DeathLink { get; internal set; }
        public static int DeathLinkGraceCount { get; internal set; }
        public static bool QuestJuggler { get; internal set; }
        public static bool QuestPacifist { get; internal set; }
        public static bool QuestProfessional { get; internal set; }
        public static int StartMaxHealth { get; internal set; }
        public static int StartMaxHealthP2 { get; internal set; }
        public static bool TrapLoadoutAnyWeapon { get; internal set; }
        public static int[] RequiredContracts { get; internal set; }
        public static int DLCRequiredIngredients { get; internal set; }
        public static int ContractsGoal { get; internal set; }
        public static int DLCIngredientsGoal { get; internal set; }
        public static bool DLCRandomizeBoat { get; internal set; }
        public static bool DLCRequiresMausoleum { get; internal set; }
        public static ItemGroups DLCChaliceItemsSeparate { get; internal set; }
        public static DlcChaliceModes DLCChaliceMode { get; internal set; }
        public static DlcChaliceCheckModes DLCBossChaliceChecks { get; internal set; }
        public static DlcChaliceCheckModes DLCRunGunChaliceChecks { get; internal set; }
        public static bool DLCDicePalaceChaliceChecks { get; internal set; }
        public static bool DLCChessChaliceChecks { get; internal set; }
        public static DlcCurseModes DLCCurseMode { get; internal set; }
        public static bool AllowGameDjimmi { get; internal set; }
        public static bool ShowUnaccessibleIslesInList { get; internal set; }
        public static MusicGroups ShuffleMusic { get; internal set; }
        public static bool RandomizeAimAbilities { get; internal set; }
        public static ShopModes ShopMode { get; internal set; }
        public static bool DuckLockPlatDropBug { get; internal set; }

        static APSettings() => Init();
        public static void Init() {
            UseDLC = false;
            Mode = GameModes.BeatDevil;
            Hard = false;
            StartWeapon = APItem.weapon_peashooter;
            WeaponMode = 0;
            FreemoveIsles = false;
            RandomizeAbilities = false;
            BossSecretChecks = false;
            BossGradeChecks = GradeChecks.Disabled;
            RungunGradeChecks = GradeChecks.Disabled;
            DicePalaceBossSanity = false;
            DeathLink = DeathLinkMode.Disabled;
            DeathLinkGraceCount = 0;
            QuestJuggler = true;
            QuestPacifist = false;
            QuestProfessional = false;
            StartMaxHealth = 3;
            StartMaxHealthP2 = StartMaxHealth;
            TrapLoadoutAnyWeapon = false;
            RequiredContracts = [5, 10, 17];
            DLCRequiredIngredients = 5;
            ContractsGoal = RequiredContracts[2];
            DLCIngredientsGoal = DLCRequiredIngredients;
            DLCRandomizeBoat = true;
            DLCRequiresMausoleum = true;
            DLCChaliceItemsSeparate = ItemGroups.None;
            DLCChaliceMode = DlcChaliceModes.Vanilla;
            DLCBossChaliceChecks = DlcChaliceCheckModes.Disabled;
            DLCRunGunChaliceChecks = DlcChaliceCheckModes.Disabled;
            DLCDicePalaceChaliceChecks = false;
            DLCChessChaliceChecks = false;
            DLCCurseMode = DlcCurseModes.Normal;
            AllowGameDjimmi = false;
            ShowUnaccessibleIslesInList = false;
            ShuffleMusic = MusicGroups.None;
            RandomizeAimAbilities = false;
            ShopMode = ShopModes.Tiers;
            DuckLockPlatDropBug = false;
        }

        public static bool IsItemGroupChaliceSeparate(ItemGroups group, bool anybit = false) {
            if (anybit) return (DLCChaliceItemsSeparate & group) > group;
            else return (DLCChaliceItemsSeparate & group) == group;
        }
    }
}

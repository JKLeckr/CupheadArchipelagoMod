/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using BepInEx.Configuration;

namespace CupheadArchipelago.Config {
    internal class MConf(ConfigFile config) {
        private ConfigEntry<byte> configTestMode = config.Bind<byte>("Main", "TestMode", 0, "Launches the mod in test mode. Keep it at 0 unless you know what you are doing!");
        private ConfigEntry<bool> configModLogs = config.Bind("Logging", "ModLogFiles", true, "Writes mod logs to files. They are not overwritten on startup unlike the main BepInEx log (unless configured not to).");
        private ConfigEntry<int> configModFileMax = config.Bind("Logging", "ModLogFileMax", 10, "The maxmum amount of mod logs that can be in the mod logs folder at once. Oldest gets deleted when max is reached.");
        private ConfigEntry<LoggingFlags> configLoggingFlags = config.Bind("Logging", "LoggingFlags", LoggingFlags.PluginInfo | LoggingFlags.Info | LoggingFlags.Message | LoggingFlags.Warning | LoggingFlags.Network, "Set mod logging verbosity.");
        private ConfigEntry<LicenseLogModes> configLogLicense = config.Bind("Logging", "LogLicense", LicenseLogModes.Off, "Log the copyright notice and license on load.\nFirstParty prints only the notice for this mod itself.\nAll includes third party notices for the libraries used. (Careful! Will flood the terminal and log!)");
        private ConfigEntry<string> configSaveKeyName = config.Bind("SaveConfig", "SaveKeyName", "cuphead_player_data_v1_ap_slot_",
                "Set save data prefix.\nPlease note that using Vanilla save files can cause data loss. It is recommended not to use Vanilla saves! (Vanilla: \"cuphead_player_data_v1_slot_\")");
        private ConfigEntry<Cutscenes> configSkipCutscenes = config.Bind("Game", "SkipCutscenes", Cutscenes.Intro | Cutscenes.DLCIntro, "Skip the specified cutscenes in an AP game.");
        private ConfigEntry<bool> configSkipCutscenesAPOnly = config.Bind("Game", "SkipCutscenesAPOnly", true, "Skip cutscenes only if playing an Archipelago game.");
        private ConfigEntry<bool> configFileDeleteResetsAPData = config.Bind("AP", "FileDeleteResetsAPData", true, "When the save file is deleted, the Archipelago settings gets reset.");
        private ConfigEntry<APStatsFunctions> configAPStatusFunctions = config.Bind("AP", "APStatusFunctions", APStatsFunctions.ConnectionIndicator, "Enable specific Archipelago Status HUD Functionalities. (Currently does nothing)");
        private ConfigEntry<bool> configVSyncFix = config.Bind("Fixes", "VSyncFix", true, "Enable the VSync fixes that prevent issues when playing the game on a monitor that does not have a 60hz refresh rate. The fixes will allow for faster monitors to use VSync correctly. The vsync setting will be ignored if the refresh rate is not divisible by 60. For those cases, go to your GPU driver settings to turn on vsync for Cuphead.exe.");
        private ConfigEntry<bool> configDebugAsInfo = config.Bind("Debug", "DebugAsInfo", true, "Logs mod debug messages to loglevel info instead of debug.");

        internal byte TestMode { get => configTestMode.Value; }
        internal bool ModLogs { get => configModLogs.Value; }
        internal int ModFileMax { get => configModFileMax.Value; }
        internal LoggingFlags LoggingFlags { get => configLoggingFlags.Value; }
        internal LicenseLogModes LogLicense { get => configLogLicense.Value; }
        internal string SaveKeyName { get => configSaveKeyName.Value; }
        internal Cutscenes SkipCutscenes { get => configSkipCutscenes.Value; }
        internal bool SkipCutscenesAPOnly { get => configSkipCutscenesAPOnly.Value; }
        internal bool FileDeleteResetsAPData { get => configFileDeleteResetsAPData.Value; }
        internal APStatsFunctions APStatusFunctions { get => configAPStatusFunctions.Value; }
        internal bool VSyncFix { get => configVSyncFix.Value; }
        internal bool DebugAsInfo { get => configDebugAsInfo.Value; }

        private static MConf Config { get => Plugin.Current.GetConfig(); }

        public static bool IsTesting() => GetTestMode() != 0;
        public static byte GetTestMode() => Config.TestMode;
        public static bool IsDebugLogsInfo() => Config.DebugAsInfo;
        public static bool IsVSyncFixEnabled() => Config.VSyncFix;
        public static bool IsSkippingCutscene(Cutscenes cutscene) {
            return IsSkippingCutscene(cutscene, APData.IsCurrentSlotEnabled());
        }
        public static bool IsSkippingCutscene(Cutscenes cutscene, bool apEnabledCondition) {
            if (!Config.SkipCutscenesAPOnly || apEnabledCondition)
                return (Config.SkipCutscenes & cutscene) == cutscene;
            else return false;
        }
        public static bool ResetAPConfigOnFileDelete() {
            return Config.FileDeleteResetsAPData;
        }
        public static bool IsAPStatsFunctionEnabled(APStatsFunctions function) {
            return (Config.APStatusFunctions & function) == function;
        }
    }
}

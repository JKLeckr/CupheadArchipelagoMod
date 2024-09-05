/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using BepInEx.Logging;

namespace CupheadArchipelago {
    public class Logging {
        private static bool init = false;
        private static ManualLogSource logSource;
        private static LoggingFlags loggingFlags;
        
        public static void Init(ManualLogSource logSource, LoggingFlags loggingFlags) {
            Logging.logSource = logSource;
            Logging.loggingFlags = loggingFlags;
            init = true;
        }

        public static void Log(object data) {
            Log(data, LogLevel.Info);
        }
        public static void Log(object data, LogLevel logLevel) {
            Log(data, (logLevel==LogLevel.Fatal||logLevel==LogLevel.Error)?LoggingFlags.None:LoggingFlags.Info, logLevel);
        }
        public static void Log(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Info);
        }
        public static void Log(object data, LoggingFlags requiredFlags, LogLevel logLevel) {
            if (!init) {
                throw new Exception("Logging not initialized.");
            }
            if (IsLoggingFlagsEnabled(requiredFlags)) {
                logSource.Log(logLevel, data);
            }
        }
        public static void LogMessage(object data) => LogMessage(data, LoggingFlags.Message);
        public static void LogMessage(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Message);
        }
        public static void LogWarning(object data) => LogWarning(data, LoggingFlags.Warning);
        public static void LogWarning(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Warning);
        }
        public static void LogError(object data) => LogError(data, LoggingFlags.None);
        public static void LogError(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Error);
        }
        public static void LogFatal(object data) => LogFatal(data, LoggingFlags.None);
        public static void LogFatal(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Fatal);
        }
        public static void LogDebug(object data) => LogDebug(data, LoggingFlags.Debug);
        public static void LogDebug(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Debug);
        }
        public static bool IsLoggingFlagsEnabled(LoggingFlags flags) {
            return (((int)flags)&((int)loggingFlags))==(int)flags;
        }
        public static bool IsInDebugMode() => IsLoggingFlagsEnabled(LoggingFlags.Debug);
    }
}
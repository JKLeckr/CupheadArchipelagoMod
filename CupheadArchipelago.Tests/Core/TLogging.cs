/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago;

namespace CupheadArchipelago.Tests.Core {
    internal class TLogging {
        internal const LoggingFlags LOGGING_FLAGS = (LoggingFlags)255;
        internal static string RecentLog { get; private set; } = "";
        internal static LogLevel RecentLevel { get; private set; } = LogLevel.None;

        internal static void SetupLogging() {
            if (!Logging.IsLoggingInitialized()) {
                Console.WriteLine("Initializing Logging...");
                Logging.Init(Log, LOGGING_FLAGS);
                Logging.Log("Logging Initialized");
            }
        }

        internal static void Log(LogLevel logLevel, object data) {
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Fatal) {
                Console.Error.WriteLine(data);
            } else {
                Console.WriteLine(data);
            }
            RecentLog = data.ToString();
            RecentLevel = logLevel;
        }
    }
}

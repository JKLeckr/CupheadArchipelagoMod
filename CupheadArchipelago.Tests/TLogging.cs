/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.Tests {
    internal class TLogging {
        internal const LoggingFlags LOGGING_FLAGS = (LoggingFlags)255;

        internal static void SetupLogging() {
            if (!Logging.IsLoggingInitialized()) {
                Console.WriteLine("Initializing Logging...");
                Logging.Init(null, LOGGING_FLAGS);
                Logging.Log("Logging Initialized");
            }
        }
    }
}

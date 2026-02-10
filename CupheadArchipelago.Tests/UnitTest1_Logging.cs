/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.Tests.Core;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest1_Logging {
        [SetUp]
        public void Setup() {}

        [Test]
        public void TestLog1_Logged() {
            string log = "Pass";
            Logging.Log(log);
            Assert.That(TLogging.RecentLog, Is.EqualTo(log));
        }
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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

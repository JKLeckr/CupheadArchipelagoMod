/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest1_Logging {
        [SetUp]
        public void Setup() {}

        [Test]
        public void TestLog() {
            Logging.Log("Pass");
            Assert.Pass();
        }
    }
}

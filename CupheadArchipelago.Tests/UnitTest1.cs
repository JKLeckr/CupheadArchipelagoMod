using System;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public class UnitTest1 {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void Test1() {
            Console.WriteLine("Passed!!!");
            Assert.Pass();
        }
    }
}

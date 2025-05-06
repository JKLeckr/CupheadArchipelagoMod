/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.AP;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest3_Items {
        [SetUp]
        public void Setup() {}

        public sealed class APItemMap {
            [SetUp]
            public void Setup() {}

            [Test]
            public void ItemMap_All_Items_Have_Type() {
                bool fail = false;
                
                foreach (APItem item in APItem.GetAllItems()) {
                    if (ItemMap.GetItemType(item) == APItemType.None) {
                        fail = true;
                        Logging.Log($"Item {item} failed.");
                    }
                }

                Assert.That(fail, Is.False);
            }
        }
    }
}

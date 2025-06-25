/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.Resources;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest2_Resources {
        [SetUp]
        public void Setup() { }

        public sealed class Resources_AssetReg {
            [SetUp]
            public void Setup() { }

            [Test]
            public void TestReg_No_Dups() {
                IEnumerable<string> bundles = AssetReg.GetAllRegisteredBundleNames();
                HashSet<string> registered = [];
                bool fail = false;

                foreach (string bundle in bundles) {
                    foreach (string asset in AssetReg.GetAssetNamesInBundle(bundle)) {
                        if (registered.Contains(asset)) {
                            fail = true;
                            Logging.Log($"Asset {asset} has a duplicate.");
                        }
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_Asset_Has_Type() {
                IEnumerable<string> assets = AssetReg.GetAllRegisteredAssetNames();
                bool fail = false;

                foreach (string asset in assets) {
                    if (AssetReg.GetAssetType(asset) == RAssetType.Object) {
                        fail = true;
                        Logging.Log($"Asset {asset} failed.");
                    }
                }

                Assert.That(fail, Is.False);
            }
        }
    }
}

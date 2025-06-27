/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Resources {
    public class RAsset(string name, RAssetType type) {
        public readonly string name = name;
        public readonly RAssetType type = type;
    }
}

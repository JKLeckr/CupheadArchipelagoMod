/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Resources {
    public class RAsset(string name, RAssetType type) {
        public readonly string name = name;
        public readonly RAssetType type = type;
    }
}

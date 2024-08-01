/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    internal class APItemDataComparer(bool useId = true) : IEqualityComparer<APItemData> {
        private readonly bool useId = useId;

        public bool Equals(APItemData a, APItemData b) {
            return (!useId || a.Id == b.Id) && a.Location == b.Location && a.Player == b.Player;
        }
        public int GetHashCode(APItemData item) => item.GetHashCode(useId);
    }
}

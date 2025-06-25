/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class ResourceReg {
        private static readonly HashSet<string> resources = new() {
            {"cap_dicehouse"}
        };

        public static IEnumerable<string> GetResourceNames() => resources;
        public static bool ResourceExists(string resourceName) => resources.Contains(resourceName);
    }
}

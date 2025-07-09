/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class ResourceDefs {
        // Register Resources here.
        private static readonly HashSet<string> resources = new() {
            { "testee" },
            { "cap_base" },
            { "cap_dicehouse" }
        };

        public static IEnumerable<string> GetRegisteredResources() => resources;
        public static bool ResourceExists(string resourceName) => resources.Contains(resourceName);
    }
}

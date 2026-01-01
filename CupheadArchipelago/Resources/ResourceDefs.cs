/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.Resources {
    public class ResourceDefs {
        // Register Resources here
        // Resources [ resources ]
        private static readonly HashSet<string> resources = [
            "testee",
            "cap_base",
            "cap_dicehouse"
        ];

        public static IEnumerable<string> GetRegisteredResources() => resources;
        public static bool ResourceExists(string resourceName) => resources.Contains(resourceName);
    }
}

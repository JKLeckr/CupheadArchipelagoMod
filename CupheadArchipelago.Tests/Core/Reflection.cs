/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Linq;
using System.Reflection;

namespace CupheadArchipelago.Tests.Core {
    internal class TReflection {
        public static FieldInfo[] GetFieldsFromClass(Type type, Type fieldType) {
            return [.. type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(f => f.FieldType == fieldType && f.IsInitOnly)];
        }
    }
}

/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Reflection;

namespace CupheadArchipelago.Tests {
    internal class TReflection {
        public static FieldInfo[] GetFieldsFromClass(Type type, Type fieldType) {
            return [.. type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(f => f.FieldType == fieldType && f.IsInitOnly)];
        }
    }
}

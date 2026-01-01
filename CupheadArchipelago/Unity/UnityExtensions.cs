/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Reflection;
using CupheadArchipelago.Util;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    internal static class ComponentExtensions {
        public static T CopyFrom<T>(this T comp, T other) where T : Component {
            return CopyFrom(comp, other, false);
        }
        public static T CopyFrom<T>(this T comp, T other, bool inherit) where T : Component {
            Type type = comp.GetType();
            Type otype = other.GetType();
            if (type != otype) throw new ArgumentException($"Type mismatch: {type} and {otype}");

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (!inherit) bindingFlags |= BindingFlags.DeclaredOnly;

            PropertyInfo[] pinfos = type.GetProperties(bindingFlags);

            foreach (PropertyInfo pinfo in pinfos) {
                if (pinfo.CanWrite && pinfo.IsObsolete()) {
                    try {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    } catch {
                        Logging.LogWarning($"Could not copy property {pinfo.Name} for {type.Name}");
                    };
                }
            }

            FieldInfo[] finfos = type.GetFields(bindingFlags);
            foreach (FieldInfo finfo in finfos) {
                if (finfo.IsObsolete()) {
                    finfo.SetValue(comp, finfo.GetValue(other));
                }
            }

            return comp;
        }
    }
}

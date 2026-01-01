/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.Util {
    internal static class Converter {
        private static class TypeConverter<T> {
            internal static readonly Func<object, T> ConvertTo;

            static TypeConverter() {
                Type T_Type = typeof(T);

                if (T_Type == typeof(string)) {
                    ConvertTo = value => (T)(object)value.ToString();
                }
                else if (T_Type == typeof(bool)) {
                    ConvertTo = value => (T)(object)Convert.ToBoolean((long)value);
                }
                else if (T_Type == typeof(sbyte)) {
                    ConvertTo = value => (T)(object)Convert.ToSByte(value);
                }
                else if (T_Type == typeof(int)) {
                    ConvertTo = value => (T)(object)Convert.ToInt32(value);
                }
                else if (T_Type.IsEnum) {
                    ConvertTo = value => (T)Enum.Parse(T_Type, value.ToString());
                }
                else {
                    ConvertTo = value => (T)value;
                }
            }
        }

        internal static T ConvertTo<T>(this object value) {
            return TypeConverter<T>.ConvertTo(value);
        }
    }
}

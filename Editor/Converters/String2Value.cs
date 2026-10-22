using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MagicExcel.Editor {
    internal class String2Value {
        private readonly Dictionary<Type, Func<string, object>> cacheConverter = new();

        private object String2Object(string input, Type targetType) {
            if (targetType == typeof(string))
                return input;

            input = input.Trim();

            if (targetType.IsEnum)
                return Enum.Parse(targetType, input, ignoreCase: true);

            if (targetType == typeof(bool)) {
                if (int.TryParse(input, out int intValue)) {
                    return intValue != 0;
                }
            }

            if (!cacheConverter.TryGetValue(targetType, out var converter)) {
                // Primitive + decimal + DateTime...
                var typeConverter = TypeDescriptor.GetConverter(targetType);
                if (typeConverter != null && typeConverter.CanConvertFrom(typeof(string))) {
                    converter = s => typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, s);
                    cacheConverter[targetType] = converter;
                }
                // Parse(string)
                var parse = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(string) }, null);
                if (parse != null) {
                    converter = s => parse.Invoke(null, new object[] { s });
                    cacheConverter[targetType] = converter;
                }
            }
            if (converter != null) {
                return converter(input);
            }
            throw new InvalidOperationException($"Cannot convert '{input}' to {targetType.FullName}");
        }

        /// <summary>
        /// Convert string to array (eg 1, 2, 3 to array int[] { 1, 2, 3 })
        /// </summary>
        private Array String2ArrayObject(string sourceValue, Type elementType) {
            var lineSeparators = new string[] { ",", ";", "\r\n", "\r", "\n" };
            var tokens = sourceValue.Split(lineSeparators, StringSplitOptions.None);
            Array array = Array.CreateInstance(elementType, tokens.Length);
            for (int i = 0; i < tokens.Length; i++) {
                object value = String2Object(tokens[i], elementType);
                array.SetValue(value, i);
            }
            return array;
        }

        public object Convert(Type type, string sourceValue) {
            if (type.IsArray) {
                return String2ArrayObject(sourceValue, type.GetElementType());
            } else {
                return String2Object(sourceValue, type);
            }
        }
    }
}

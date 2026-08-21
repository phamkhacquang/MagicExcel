using System;
using System.Collections.Generic;

namespace MagicExcel {
    public static class ArrayExtensions {
        public static T GetBy<T>(this IList<T> data, Func<T, bool> predicate, ErrorReturn errorReturn = ErrorReturn.Null) {
            if (data == null || data.Count == 0) {
                return default;
            } else {
                foreach (var item in data) {
                    if (predicate(item)) {
                        return item;
                    }
                }
            }
            return errorReturn switch {
                ErrorReturn.FirstValue => data[0],
                ErrorReturn.LastValue => data[^1],
                _ => default
            };
        }

        public static T MinBy<T>(this IList<T> data, Func<T, IComparable> selector) {
            if (data == null || data.Count == 0) {
                return default;
            } else {
                T minItem = data[0];
                IComparable minValue = selector(minItem);
                for (int i = 1; i < data.Count; i++) {
                    var currentValue = selector(data[i]);
                    if (currentValue.CompareTo(minValue) < 0) {
                        minValue = currentValue;
                        minItem = data[i];
                    }
                }
                return minItem;
            }
        }

        public static T MaxBy<T>(this IList<T> data, Func<T, IComparable> selector) {
            if (data == null || data.Count == 0) {
                return default;
            } else {
                T maxItem = data[0];
                IComparable maxValue = selector(maxItem);
                for (int i = 1; i < data.Count; i++) {
                    var currentValue = selector(data[i]);
                    if (currentValue.CompareTo(maxValue) > 0) {
                        maxValue = currentValue;
                        maxItem = data[i];
                    }
                }
                return maxItem;
            }
        }
    }

    public enum ErrorReturn {
        Null,
        FirstValue,
        LastValue
    }
}
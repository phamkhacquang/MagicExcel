using System;

namespace MagicExcel.Editor {
    internal class FieldDataSource {
        public readonly string type;
        public readonly string name;
        public readonly string constValue;
        public readonly bool isArray;

        public FieldDataSource(string type, string name, string constValue) {
            this.type = type;
            this.name = name;
            isArray = type.EndsWith("[]");

            if (!string.IsNullOrEmpty(constValue)) {
                if (type == "string") {
                    this.constValue = $"\"{constValue}\"";
                } else if (type == "string[]") {
                    var strings = constValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strings.Length; i++) {
                        strings[i] = $"\"{strings[i]}\"";
                    }
                    this.constValue = string.Join(", ", strings);
                } else if (type == "float" && !constValue.EndsWith("f")) {
                    this.constValue = $"{constValue}f";
                } else if (type == "float[]") {
                    var numbers = constValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < numbers.Length; i++) {
                        if (!numbers[i].EndsWith("f")) {
                            numbers[i] += "f";
                        }
                    }
                    this.constValue = string.Join(", ", numbers);
                } else {
                    this.constValue = constValue;
                }
            }
        }
    }
}
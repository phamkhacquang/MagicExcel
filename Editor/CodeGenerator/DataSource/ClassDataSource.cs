using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicExcel.Editor {
    internal class ClassDataSource {
        public readonly string className;
        public readonly string @namespace;
        public readonly string[] usingDirectives;
        public readonly bool isConstClass;
        public readonly List<FieldDataSource> fields;

        public ClassDataSource(Page page, Setting setting) {
            className = page.GetClassName(setting);
            @namespace = setting.scriptNamespace;
            usingDirectives = setting.additionalNamespaces
                .Append("System").Append("UnityEngine").Distinct().ToArray();
            isConstClass = page.IsConstClass();
            fields = GetFieldsFromPage(page, isConstClass);
        }

        public static List<FieldDataSource> GetFieldsFromPage(Page page, bool isConstClass) {
            var fields = new List<FieldDataSource>();
            //first row: int XXX or string[] YYY or ZZZ(=string ZZZ)
            for (int col = 0; col < page.data.GetLength(1); col++) {
                var d = page.data[0, col].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (d.Length == 0) {
                    page.LogError("Invalid field definition", 0, col);
                    break;
                }
                var type = d.Length == 1 ? "string" : d[0];
                var name = d.Length == 1 ? d[0] : d[1];
                fields.Add(new FieldDataSource(type, name, isConstClass ? page.data[1, col] : null));
            }
            return fields;
        }
    }
}
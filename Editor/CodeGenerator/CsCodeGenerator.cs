using HandlebarsDotNet;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.PackageManager;

namespace MagicExcel.Editor {
    internal class CsCodeGenerator {
        private readonly string packagePath;
        private readonly HandlebarsTemplate<object, object> classTemplate;
        private readonly HandlebarsTemplate<object, object> constClassTemplate;
        private readonly HandlebarsTemplate<object, object> excelDataTemplate;
        private readonly IHandlebars handlebars;

        private string GetPath(string filename)
            => Path.Combine(packagePath, "Editor/CodeGenerator/Template", filename);

        public CsCodeGenerator() {
            packagePath = PackageInfo.FindForAssembly(typeof(CsCodeGenerator).Assembly).resolvedPath;
            handlebars = Handlebars.Create(new HandlebarsConfiguration { TextEncoder = null });
            var txt = File.ReadAllText(GetPath("Class.template"), Encoding.UTF8);
            classTemplate = handlebars.Compile(txt);
            txt = File.ReadAllText(GetPath("ConstClass.template"), Encoding.UTF8);
            constClassTemplate = handlebars.Compile(txt);
            txt = File.ReadAllText(GetPath("ExcelData.template"), Encoding.UTF8);
            excelDataTemplate = handlebars.Compile(txt);
        }

        public bool Generate(ClassDataSource classDataSource, string folder) {
            var code = classDataSource.isConstClass
                ? constClassTemplate(classDataSource)
                : classTemplate(classDataSource);

            var filePath = Path.Combine(folder, classDataSource.className + ".cs");
            if (File.Exists(filePath)) {
                var oldCode = File.ReadAllText(filePath, Encoding.UTF8);
                if (oldCode == code) {
                    return false;
                }
            }
            File.WriteAllText(filePath, code, Encoding.UTF8);
            return true;
        }

        public bool GenerateSet(Dictionary<string, string> nonConstClass, string scriptNamespace, string folder) {
            var code = excelDataTemplate(new { @namespace = scriptNamespace, fields = nonConstClass });
            var filePath = Path.Combine(folder, Setting.DATA_SET_NAME + ".cs");
            if (File.Exists(filePath)) {
                var oldCode = File.ReadAllText(filePath, Encoding.UTF8);
                if (oldCode == code) {
                    return false;
                }
            }
            File.WriteAllText(filePath, code, Encoding.UTF8);
            return true;
        }
    }
}
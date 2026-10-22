using System;
using UnityEditor;
using UnityEngine;

namespace MagicExcel.Editor {
    [FilePath("ProjectSettings/MagicExcelSetting.asset", FilePathAttribute.Location.ProjectFolder)]
    public class Setting : ScriptableSingleton<Setting> {

        public const string DATA_SET_NAME = "ExcelData";
        public const string IGNORE_PREFIX = "Ignore";
        public const string SETTING_SUFFIX = "Setting";

        public string[] spreadsheetIds = new string[0];
        [Space]
        public string scriptNamespace = "Excel";
        public string[] additionalNamespaces;
        public string classNameFormat;
        public SheetMapping[] sheetCustomMappings;
        [Space]
        public string outputFolder = "Assets/ExcelData";
        public string ScriptsOutputFolder => outputFolder + "/Scripts";
        public string AssetOutputFolder => outputFolder + "/Assets";

        public void Save() {
            Save(true);
        }
    }

    [Serializable]
    public class SheetMapping {
        public string className;
        public string[] sheetNames;
    }
}

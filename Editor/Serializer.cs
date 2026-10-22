using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MagicExcel.Editor {
    internal static class Serializer {
        public static void Serialize(List<Page> pages, Setting setting) {
            string dataSetName = Setting.DATA_SET_NAME;
            var dataSetType = GetDataSetType(setting.scriptNamespace);
            if (dataSetType == null) {
                Debug.LogError($"Cannot find type {dataSetName} in namespace {setting.scriptNamespace}");
                return;
            }
            if (!Directory.Exists(setting.AssetOutputFolder)) {
                Directory.CreateDirectory(setting.AssetOutputFolder);
            }
            ScriptableObject dataSetSO = ScriptableObject.CreateInstance(dataSetType);
            dataSetSO.name = dataSetName;
            FillData(pages, setting, dataSetSO, dataSetType);
            var assetPath = Path.Combine(setting.AssetOutputFolder, dataSetName + ".asset");
            if (File.Exists(assetPath)) {
                UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(assetPath);
                EditorUtility.CopySerialized(dataSetSO, old);
                Debug.Log("Completed, update: " + assetPath.Replace("\\", "/"), dataSetSO);
            } else {
                AssetDatabase.CreateAsset(dataSetSO, assetPath);
                Debug.Log("Completed, create: " + assetPath.Replace("\\", "/"), dataSetSO);
            }
        }

        private static Type GetDataSetType(string scriptNameSpace) {
            var typeName = string.IsNullOrWhiteSpace(scriptNameSpace)
                ? Setting.DATA_SET_NAME
                : scriptNameSpace + "." + Setting.DATA_SET_NAME;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = assembly.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }
            return null;
        }

        private static void FillData(List<Page> pages, Setting setting, ScriptableObject dataSetSO, Type dataSetType) {
            String2Value converter = new();
            foreach (var page in pages) {
                var soFieldName = "m_" + page.name;
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                FieldInfo fieldInfo = dataSetType.GetField(soFieldName, flags);
                if (fieldInfo == null) {
                    Debug.LogError($"Cannot find field {soFieldName} in {dataSetType.FullName}");
                    continue;
                }
                Type classType = fieldInfo.FieldType.GetElementType();
                Array configs = Source2ArrayObject(classType, page, converter);
                if (configs != null) {
                    fieldInfo.SetValue(dataSetSO, configs);
                }
            }
        }

        private static Array Source2ArrayObject(Type classType, Page page, String2Value converter) {
            var total = page.data.GetLength(0) - 1;
            var fieldDatas = ClassDataSource.GetFieldsFromPage(page, false);
            Array configs = Array.CreateInstance(classType, total);
            FieldInfo[] fields = new FieldInfo[fieldDatas.Count];
            for (int i = 0; i < fieldDatas.Count; i++) {
                string backingFieldName = $"<{fieldDatas[i].name}>k__BackingField";
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                fields[i] = classType.GetField(backingFieldName, flags);
                if (fields[i] == null) {
                    Debug.LogError($"Cannot find field {fieldDatas[i].name} in {classType.FullName}");
                    return null;
                }
            }
            for (int i = 0; i < total; i++) {
                object obj = Activator.CreateInstance(classType);
                for (int j = 0; j < fields.Length; j++) {
                    string raw = page.data[i + 1, j];
                    try {
                        object value = converter.Convert(fields[j].FieldType, raw);
                        fields[j].SetValue(obj, value);
                    } catch (Exception ex) {
                        fields[j].SetValue(obj, default);
                        page.LogError($"Cannot convert '{raw}' to {fields[j].FieldType.FullName}: {ex.Message}", i + 1, j);
                    }
                }
                configs.SetValue(obj, i);
            }
            return configs;
        }
    }
}
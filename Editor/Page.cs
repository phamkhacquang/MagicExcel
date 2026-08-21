using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MagicExcel.Editor {
    public class PagesWrapper {
        public readonly List<Page> pages;

        public PagesWrapper(List<Page> pages) {
            this.pages = pages;
        }

        public void SaveToFile(string path) {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bw = new BinaryWriter(fs, Encoding.UTF8);

            bw.Write(pages.Count);
            foreach (var page in pages) {
                bw.Write(page.name);

                var data = page.data;
                if (data == null) {
                    bw.Write(0);
                    bw.Write(0);
                } else {
                    int rows = data.GetLength(0);
                    int cols = data.GetLength(1);
                    bw.Write(rows);
                    bw.Write(cols);
                    for (int r = 0; r < rows; r++) {
                        for (int c = 0; c < cols; c++) {
                            var cell = data[r, c];
                            if (cell == null) {
                                bw.Write(false);
                            } else {
                                bw.Write(true);
                                bw.Write(cell);
                            }
                        }
                    }
                }
            }
        }

        public static PagesWrapper LoadFromFile(string path) {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs, Encoding.UTF8);

            int pageCount = br.ReadInt32();
            var pages = new List<Page>(pageCount);
            for (int i = 0; i < pageCount; i++) {
                var name = br.ReadString();

                int rows = br.ReadInt32();
                int cols = br.ReadInt32();
                string[,] data;
                if (rows > 0 && cols > 0) {
                    data = new string[rows, cols];
                    for (int r = 0; r < rows; r++) {
                        for (int c = 0; c < cols; c++) {
                            bool has = br.ReadBoolean();
                            data[r, c] = has ? br.ReadString() : null;
                        }
                    }
                } else {
                    data = new string[0, 0];
                }

                pages.Add(new Page(name, data));
            }

            return new PagesWrapper(pages);
        }
    }

    [Serializable]
    public class Page {
        public readonly string name;
        public readonly string[,] data;

        public Page(string name, string[,] data) {
            this.name = name;
            this.data = data;
        }

        public string GetClassName(Setting setting) {
            for (int i = 0; i < setting.sheetCustomMappings.Length; i++) {
                var mapping = setting.sheetCustomMappings[i];
                if (mapping.sheetNames != null && Array.Exists(mapping.sheetNames, s => s == name)) {
                    return mapping.className;
                }
            }
            if (string.IsNullOrEmpty(setting.classNameFormat)) {
                return name;
            } else {
                return string.Format(setting.classNameFormat, name);
            }
        }

        public bool IsConstClass() => name.EndsWith(Setting.SETTING_SUFFIX);

        public void LogError(string err, int row, int col) {
            Debug.LogError($"{name}:{ConvertToExcelColumn(col)}{row + 1} - {err}");
        }

        private static string ConvertToExcelColumn(int column) {
            string columnS = string.Empty;
            while (column >= 0) {
                int remainder = column % 26;
                columnS = (char)(remainder + 'A') + columnS;
                column = column / 26 - 1;
            }
            return columnS;
        }
    }
}
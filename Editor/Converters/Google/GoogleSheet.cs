using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicExcel.Editor.Converters.Google {
    internal static class GoogleSheet {
        internal static async Task<List<Page>> LoadData(string[] spreadsheetIds) {
            HashSet<string> uniqueSheetNames = new();
            List<Page> allPages = new();
            foreach (var spreadsheetId in spreadsheetIds) {
                var pages = await LoadData(spreadsheetId);
                foreach (var page in pages) {
                    if (uniqueSheetNames.Add(page.name)) {
                        allPages.Add(page);
                    } else {
                        Debug.LogWarning($"Duplicate sheet name '{page.name}' found in spreadsheet '{spreadsheetId}'. Skipping this sheet.");
                    }
                }
            }
            return allPages;
        }

        internal static async Task<List<Page>> LoadData(string spreadsheetId) {
            try {
                using var stream = await DownloadSpreadsheetAsync(spreadsheetId);
                return ReadPages(stream, spreadsheetId);
            } catch (Exception ex) {
                Debug.LogError($"Failed to read spreadsheet '{spreadsheetId}'. Error: {ex.Message}");
                return new List<Page>();
            }
        }

        private static async Task<MemoryStream> DownloadSpreadsheetAsync(string spreadsheetId) {
            using var client = new WebClient();
            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
            var url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=xlsx&t={DateTime.UtcNow.Ticks}";
            var bytes = await client.DownloadDataTaskAsync(url);
            return new MemoryStream(bytes, writable: false);
        }

        private static List<Page> ReadPages(Stream stream, string spreadsheetId) {
            using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            List<Page> result = new();

            do {
                var sheetName = string.IsNullOrEmpty(reader.Name) ? "Sheet" : reader.Name;
                if (sheetName.StartsWith(Setting.IGNORE_PREFIX)) {
                    continue;
                }

                var rows = new List<string[]>();
                int columnCount = 0;

                while (reader.Read()) {
                    if (columnCount == 0) {
                        columnCount = GetColumnCount(reader);
                    }

                    if (columnCount == 0) {
                        continue;
                    }

                    var row = new string[columnCount];
                    bool allEmpty = true;
                    for (int colIndex = 0; colIndex < columnCount; colIndex++) {
                        row[colIndex] = GetCellValue(reader, colIndex);
                        if (!string.IsNullOrEmpty(row[colIndex])) {
                            allEmpty = false;
                        }
                    }

                    if (allEmpty) {
                        // If entire row is empty, skip it and stop reading this sheet
                        break;
                    }

                    rows.Add(row);
                }

                if (rows.Count == 0) {
                    Debug.LogWarning($"Sheet '{sheetName}' in spreadsheet '{spreadsheetId}' has no rows.");
                    continue;
                }

                if (rows.Count < 2 || columnCount == 0) {
                    Debug.LogWarning($"Sheet '{sheetName}' in spreadsheet '{spreadsheetId}' is empty or has no valid data.");
                    continue;
                }

                var dataMatrix = new string[rows.Count, columnCount];
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
                    for (int colIndex = 0; colIndex < columnCount; colIndex++) {
                        dataMatrix[rowIndex, colIndex] = rows[rowIndex][colIndex];
                    }
                }

                result.Add(new Page(sheetName, dataMatrix));
            } while (reader.NextResult());

            if (result.Count == 0) {
                throw new InvalidOperationException($"Spreadsheet '{spreadsheetId}' is null or empty");
            }

            return result;
        }

        private static int GetColumnCount(IExcelDataReader reader) {
            for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++) {
                if (string.IsNullOrEmpty(GetCellValue(reader, columnIndex))) {
                    return columnIndex;
                }
            }

            return reader.FieldCount;
        }

        private static string GetCellValue(IExcelDataReader reader, int columnIndex) {
            if (columnIndex >= reader.FieldCount) {
                return string.Empty;
            }

            var value = reader.GetValue(columnIndex);
            return value switch {
                null => string.Empty,
                string s => s,
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty,
            };
        }
    }
}

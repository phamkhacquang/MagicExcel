using MagicExcel.Editor.Converters.Google;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicExcel.Editor {
    internal class MagicExcelWindow : EditorWindow {
        public const string TMP_DATA_FILE = "Temp/MagicExcelTempData.bin";

        private Setting setting;

        [MenuItem("Window/Magic Excel")]
        public static MagicExcelWindow Get() => GetWindow<MagicExcelWindow>("Magic Excel");

        public void CreateGUI() {
            setting = Setting.instance;
            rootVisualElement.Clear();

            VisualElement main = new();
            main.style.flexGrow = 1;

            var box = new VisualElement { name = "Box" };
            box.AddToClassList("unity-box");
            box.style.paddingLeft = box.style.paddingRight = box.style.paddingTop = box.style.paddingBottom = 8;
            box.style.marginRight = box.style.marginLeft = box.style.marginTop = box.style.marginBottom = 5;
            main.Add(box);

            var header = new VisualElement {
                style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                marginBottom = 8 }
            };
            header.Add(new Label("Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            header.Add(new Button(OpenDocumentation) { text = "Documentation ↗" });
            box.Add(header);

            GUISettings(box);
            Color greenColor = new(0.1f, 0.6f, 0.2f, 1f);
            main.Add(new Button(Output) {
                text = "Output",
                style = {
                    backgroundColor = greenColor,
                    color = Color.white,
                    width = 200, height = 30,
                    marginLeft = StyleKeyword.Auto,
                    marginRight = StyleKeyword.Auto }
            });

            rootVisualElement.Add(main);
            rootVisualElement.Add(GUICopyright());
        }

        private void OnEnable() {
            // Rebind setting and rebuild UI when the window is enabled (covers assembly reloads)
            setting = Setting.instance;
            if (rootVisualElement != null) {
                rootVisualElement.Clear();
                CreateGUI();
            }
        }

        private void OpenDocumentation() {
            Application.OpenURL("http://github.com/phamkhacquang/MagicExcel");
        }

        private bool isTrackingSerializedObjectValue = false;
        private void GUISettings(VisualElement element) {
            var serializedObject = new SerializedObject(setting);
            var inspectorElement = new InspectorElement(serializedObject);
            element.Add(inspectorElement);
            var scriptField = inspectorElement.Q("PropertyField:m_Script");
            if (scriptField != null) {
                scriptField.style.display = DisplayStyle.None;
            }
            if (!isTrackingSerializedObjectValue) {
                rootVisualElement.Bind(serializedObject);
                rootVisualElement.TrackSerializedObjectValue(serializedObject, so => {
                    so.ApplyModifiedProperties();
                    setting.Save();
                });
                isTrackingSerializedObjectValue = true;
            }
        }

        private VisualElement GUICopyright() {
            Label copyright = new("©quangpk");
            copyright.style.color = Color.gray;
            copyright.style.marginLeft = 5;
            copyright.style.marginBottom = 5;
            return copyright;
        }

        private static void ProgressBar(string info, float progress) {
            EditorUtility.DisplayProgressBar("Magic Excel", info, progress);
        }

        private async void Output() {
            if (setting.spreadsheetIds.Length == 0) {
                EditorUtility.DisplayDialog("Error", "At least one Spreadsheet ID must be set.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(setting.scriptNamespace)) {
                EditorUtility.DisplayDialog("Error", "Script Namespace must be set.", "OK");
                return;
            }
            ProgressBar("Loading data from Google Sheets...", 0.25f);
            try {
                var pages = await GoogleSheet.LoadData(setting.spreadsheetIds);
                ProcessData(pages);
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ProcessData(List<Page> pages) {
            var percentPerPage = 0.5f / pages.Count;
            var needCompile = false;

            var csCodeGenerator = new CsCodeGenerator();
            if (!Directory.Exists(setting.ScriptsOutputFolder)) {
                Directory.CreateDirectory(setting.ScriptsOutputFolder);
            }

            HashSet<string> createdClassNames = new();
            for (var i = 0; i < pages.Count; i++) {
                var page = pages[i];
                if (createdClassNames.Add(page.GetClassName(setting))) {
                    ProgressBar($"Processing page {page.name}...", 0.25f + percentPerPage * i);
                    var classD = new ClassDataSource(page, setting);
                    needCompile = csCodeGenerator.Generate(classD, setting.ScriptsOutputFolder) || needCompile;
                }
            }

            var nonConstPages = pages.FindAll(p => !p.IsConstClass());
            var nonConstClass = nonConstPages.ToDictionary(p => p.name, p => p.GetClassName(setting));
            needCompile = csCodeGenerator.GenerateSet(nonConstClass, setting.scriptNamespace, setting.ScriptsOutputFolder) || needCompile;

            if (needCompile) {
                PagesWrapper pagesWrapper = new(nonConstPages);
                pagesWrapper.SaveToFile(TMP_DATA_FILE);
                Debug.Log("Magic Excel: Waiting for compilation to finish...");
            } else {
                if (File.Exists(TMP_DATA_FILE)) {
                    File.Delete(TMP_DATA_FILE);
                }
                Serializer.Serialize(nonConstPages, setting);
            }
            AssetDatabase.Refresh();
        }
    }
}
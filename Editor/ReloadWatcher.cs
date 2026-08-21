using System.IO;
using UnityEditor;

namespace MagicExcel.Editor {
    [InitializeOnLoad]
    public static class ReloadWatcher {
        static ReloadWatcher() {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
        }

        static void OnAfterReload() {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (File.Exists(MagicExcelWindow.TMP_DATA_FILE)) {
                PagesWrapper pagesWrapper = PagesWrapper.LoadFromFile(MagicExcelWindow.TMP_DATA_FILE);
                File.Delete(MagicExcelWindow.TMP_DATA_FILE);
                Setting setting = Setting.instance;
                Serializer.Serialize(pagesWrapper.pages, setting);
            }
        }
    }
}
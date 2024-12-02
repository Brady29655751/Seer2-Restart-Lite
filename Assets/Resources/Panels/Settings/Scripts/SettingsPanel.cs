using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class SettingsPanel : Panel
{
    [SerializeField] private VolumeSettingController volumeController;

    public void ExportRole() {
        var filter = new FileBrowser.Filter("save", ".xml");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowSaveDialog(OnExportSuccess, OnExportCancel, FileBrowser.PickMode.Files,
            initialFilename: "save" + Player.instance.gameDataId, title: "选择要导出的位置并命名");
    }

    private void OnExportSuccess(string[] paths) {
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部保存位置并命名，而非默认存档位置", 16);
            return;
        }

        bool isSuccessSaving = SaveSystem.TrySaveXML(Player.instance.gameData, paths[0], out var message);
        if (isSuccessSaving)
            message = "导出成功";

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(message, 20, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }

    private void OnExportCancel() {

    }

    public void OpenWorkshopPanel() {
        Panel.OpenPanel<WorkshopPanel>();
    }
    
    public void QuitGame() {
        Application.Quit();
    }
}

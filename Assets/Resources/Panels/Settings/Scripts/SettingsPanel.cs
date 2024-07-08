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
        FileBrowser.ShowSaveDialog(OnExportSuccess, OnExportCancel, FileBrowser.PickMode.Files, title: "选择要导出的位置");
    }

    private void OnExportSuccess(string[] paths) {
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

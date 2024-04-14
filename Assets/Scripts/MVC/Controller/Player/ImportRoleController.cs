using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class ImportRoleController : Module
{
    [SerializeField] private Text otherUserLoginText;

    public bool isImporting { get; private set; } = false;
    private int importId = -1;

    public void Import() {
        if (isImporting)
            return;

        isImporting = true;
        otherUserLoginText?.SetText("选择要覆盖的存档");
    }

    public void Cover(int id) {
        if (!isImporting)
            return;

        importId = id;

        var filter = new FileBrowser.Filter("save", ".xml");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(OnSuccess, OnCancel, FileBrowser.PickMode.Files, initialPath: Application.persistentDataPath + "/../", title: "选择要导入的存档");
    }

    private void OnSuccess(string[] paths) {
        bool isSuccessSaving = SaveSystem.TryLoadXML<GameData>(paths[0], out var importData, out var message);
        if (isSuccessSaving) {
            SaveSystem.SaveData(importData, importId);
            message = "导入成功";
        }

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(message, 20, FontOption.Arial);
        hintbox.SetOptionNum(1);
        hintbox.SetOptionCallback(() => {
            if (isSuccessSaving)
                SceneLoader.instance.ChangeScene(SceneId.Title);
        });
        OnCancel();
    }

    private void OnCancel() {
        isImporting = false;
        otherUserLoginText?.SetText("导入其他存档");
    }
    
    
}

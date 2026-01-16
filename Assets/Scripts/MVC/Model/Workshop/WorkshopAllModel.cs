using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class WorkshopAllModel : Module
{
    [SerializeField] private WorkshopLearnSkillModel learnSkillModel;
    [SerializeField] private WorkshopLearnBuffModel learnBuffModel;
    [SerializeField] private WorkshopLearnItemModel learnItemModel;

    public Skill currentSkill => learnSkillModel.currentSkill;
    public BuffInfo currentBuffInfo => learnBuffModel.currentBuffInfo;
    public ItemInfo currentItemInfo => learnItemModel.currentItemInfo;
    
    public void OnImportMod() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("导入其他mod会覆盖当前mod资料\n也会失去目前所有获得的mod精灵\n<color=#ffbb33>请先确定当前mod和存档已经导出保存</color>\n若已保存请点击【确认】继续导入mod", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            FileBrowser.ShowLoadDialog(OnImportSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导入的mod（手机端请先点击左边的Browse才能浏览）");
        });
    }

    private void OnImportSuccess(string[] paths) {
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部Mod文件，而非默认存档位置的文件", 16);
            return;
        }

        var isSuccessImporting = SaveSystem.TryDeleteMod() && SaveSystem.TryImportMod(paths[0]);
        var message = isSuccessImporting ? "导入成功，请重新启动游戏" : "导入失败";
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);

        if (isSuccessImporting)
            hintbox.SetOptionCallback(Application.Quit);
    }
    
    
    public void OnExportMod() {
        var hintbox = Hintbox.OpenHintboxWithContent("导出mod会在选择的资料夹里面自动产生或覆盖一个名为Mod的资料夹，后续导入时选择该资料夹即可", 16);
        hintbox.SetOptionCallback(() => {
            FileBrowser.ShowSaveDialog(OnExportSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导出的位置（手机端请先点击左边的Browse才能浏览）");
        });
    }

    private void OnExportSuccess(string[] paths) {
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部位置，而非默认存档位置", 16);
            return;
        }

        var message = SaveSystem.TryExportMod(paths[0]) ? "导出成功" : "导出失败";
        Hintbox.OpenHintboxWithContent(message, 16);
    }

    public void OnUpdateMod() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("更新mod不会删除当前存档內容。若新旧内容冲突会造成问题，确定要继续更新吗？", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            FileBrowser.ShowLoadDialog(OnUpdateSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导入的mod（手机端请先点击左边的Browse才能浏览）");
        });
    }

    private void OnUpdateSuccess(string[] paths) {
        if ((paths[0] == Application.persistentDataPath) || (paths[0] == (Application.persistentDataPath + "/Mod")) ||
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath + "/Mod")) {
            Hintbox.OpenHintboxWithContent("需选择其他Mod文件，而非默认的Mod文件", 16);
            return;
        }

        var isSuccessImporting = SaveSystem.TryImportMod(paths[0]);
        var message = isSuccessImporting ? "更新成功，请重新启动游戏" : "更新失败";
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);

        if (isSuccessImporting)
            hintbox.SetOptionCallback(Application.Quit);
    }

    public void OnDeleteMod() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("确定要删除mod吗？\n会失去目前所有获得的mod精灵！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            bool isSuccessDeleted = SaveSystem.TryDeleteMod();
            var message = isSuccessDeleted ? "删除成功，请重新启动游戏" : "删除失败";
            var hintbox = Hintbox.OpenHintboxWithContent(message, 16);

            if (isSuccessDeleted)
                hintbox.SetOptionCallback(Application.Quit);
        });
    }

    private void OnCancel() {}
    
    
    
    
    
}

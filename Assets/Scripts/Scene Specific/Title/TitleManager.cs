using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;

public class TitleManager : Manager<TitleManager>
{
    [SerializeField] private IButton startButton;
    [SerializeField] private IText versionDateText, versionStringText;

    private void Start() {
        StartCoroutine(CheckVersionData());
    }

    private IEnumerator CheckVersionData() {
        while (GameManager.versionData == null)
            yield return null;

        if (GameManager.versionData.IsEmpty()) {
            versionDateText?.SetText("----/--/--");
            versionStringText?.SetText("----");
            yield break;
        }
        
        versionDateText?.SetText(GameManager.versionData.releaseDate.ToString("yyyy/MM/dd"));
        versionStringText?.SetText(GameManager.versionData.gameVersion);
    }

    public void GameStart() {
        if (GameManager.versionData == null) {
            RequestManager.OnRequestFail("正在获取版本档案，请稍候");
            return;
        }
        if (GameManager.versionData.IsEmpty()) {
            RequestManager.OnRequestFail("获取版本档案失败，请重新启动游戏");
            return;
        }
        if (GameManager.versionData.buildVersion != Application.version) {
            startButton?.SetInteractable(false, false);
            RequestManager.OnRequestFail("检测到新版本，正在获取更新档案大小，请稍候");
            RequestManager.instance.GetDownloadSize(GameManager.gameDownloadUrl, OpenUpdateBuildHintbox);
            return;
        }
        SceneLoader.instance.ChangeScene(SceneId.Login);
    }

    private void OpenUpdateBuildHintbox(long size) {
        startButton?.SetInteractable(true, false);

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        if (size == -1) {                
            hintbox.SetContent("检测到新版本，但获取档案大小失败，请稍后再试", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }

        hintbox.SetContent("检测到新版本。点击确认选择下载位置。\n建议预留 " + (size / 1_000_000) + " MB 以上空间。\n下载后旧版本可自行删除。", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnSelectDownloadPosition, true);
    }

    private void OnSelectDownloadPosition() {
        var downloadUrl = GameManager.gameDownloadUrl;
        var filter = new FileBrowser.Filter("Game", ".zip");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowSaveDialog(OnSelectDownloadPositionSuccess, OnCancel, FileBrowser.PickMode.Files, initialFilename: "Seer2_Restart_Lite");
    }

    private void OnSelectDownloadPositionSuccess(string[] paths) {
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(0);
        var loadingText = loadingScreen?.loadingText;
        loadingText?.SetText("正在下载新版本");
        RequestManager.instance.Download(GameManager.gameDownloadUrl, paths[0], 
            () => loadingText?.SetText("下载完成，请关闭游戏并解压缩新游戏档案\n旧版本可自行删除"),
            (error) => loadingText?.SetText("下载失败，请重新启动\n错误：" + error),
            loadingScreen.ShowLoadingProgress
        );
    }

    private void OnCancel() {}


    # region mod
    public void OnImportMod() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("导入其他mod会覆盖当前mod资料\n也会失去目前所有获得的mod精灵\n请先确定当前mod已经导出保存\n若已保存请点击【确认】继续导入mod", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            FileBrowser.ShowLoadDialog(OnImportSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导入的mod");
        });
    }

    private void OnImportSuccess(string[] paths) {
        var isSuccessImporting = SaveSystem.TryImportMod(paths[0]);
        var message = isSuccessImporting ? "导入成功，请重新启动游戏" : "导入失败";
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);

        if (isSuccessImporting)
            hintbox.SetOptionCallback(Application.Quit);
    }


    public void OnExportMod() {
        var hintbox = Hintbox.OpenHintboxWithContent("导出mod会在选择的资料夹里面自动产生或覆盖一个名为Mod的资料夹，后续导入时选择该资料夹即可", 16);
        hintbox.SetOptionCallback(() => {
            FileBrowser.ShowSaveDialog(OnExportModSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导出的位置");
        });
    }

    private void OnExportModSuccess(string[] paths) {
        var message = SaveSystem.TryExportMod(paths[0]) ? "导出成功" : "导出失败";
        Hintbox.OpenHintboxWithContent(message, 16);
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

    #endregion
}

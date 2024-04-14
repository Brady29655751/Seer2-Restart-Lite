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
        hintbox.SetOptionCallback(OpenSaveFileBrowser, true);
    }

    private void OpenSaveFileBrowser() {
        var downloadUrl = GameManager.gameDownloadUrl;
        var filter = new FileBrowser.Filter("Game", ".zip");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowSaveDialog(OnSuccess, OnCancel, FileBrowser.PickMode.Files, initialFilename: "Seer2_Restart_Lite");
    }

    private void OnSuccess(string[] paths) {
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

}

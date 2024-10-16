using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;

public class TitleManager : Manager<TitleManager>
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private List<GameObject> backgroundGadgetObjectList;
    [SerializeField] private IButton startButton;
    [SerializeField] private IText versionDateText, versionStringText;
    [SerializeField] private GameObject toolBarObject, modOperateObject;

    private void Start() {
        StartCoroutine(CheckVersionData());

        FileBrowser.AddQuickLink("默认存档位置", Application.persistentDataPath);
        SetBackgroundPageImage();
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

    private void SetBackgroundPageImage() {
        if (!SaveSystem.IsModExists())
            return;
        
        var modBackgroundSprite = ResourceManager.instance.GetLocalAddressables<Sprite>("Activities/FirstPage", true);
        if (modBackgroundSprite == null) 
            return;
        
        backgroundImage.SetSprite(modBackgroundSprite);
        backgroundImage.color = Color.white;
        backgroundGadgetObjectList.ForEach(x => x?.SetActive(false));
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
        if (!SaveSystem.IsResourcesExists()) {
            var hintbox = Hintbox.OpenHintbox();
            hintbox.SetSize(540, 360);
            hintbox.SetTitle("获取资源档案失败");
            hintbox.SetContent("请到群内下载「基础资源包」\n解压后点击右下方的「导入资源包」按钮\n手动导入里面名为Resources的文件夹\n\n" +
                "若要新增动画，同样到群内下载对应版本的「动画资源包」\n详情请查看群公告\n\n" +
                "导入过程会花费大约10秒左右，请耐心等待\n若长时间无反应或卡在地图加载页面，可能为文件缺失，请重新正确导入", 16, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        SceneLoader.instance.ChangeScene(SceneId.Login);
    }

    private void OnCancel() {}

    #region update

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

    #endregion

    #region resource

    public void OnImportResources() {
        FileBrowser.ShowLoadDialog(OnResourceSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导入的资源（手机端请先点击左边的Browse才能浏览）");
    }

    private void OnResourceSuccess(string[] paths) {
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部文件，而非默认存档位置文件", 16);
            return;
        }

        var isSuccessImporting = SaveSystem.TryImportResources(paths[0], out var error);
        var message = isSuccessImporting ? "导入成功" : ("导入失败\n" + error);
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);
    }

    #endregion

    # region mod

    public void OnOperateMod() {
        toolBarObject?.SetActive(false);
        modOperateObject?.SetActive(true);
    }

    public void OnImportMod() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("导入其他mod会覆盖当前mod资料\n也会失去目前所有获得的mod精灵\n请先确定当前mod已经导出保存\n若已保存请点击【确认】继续导入mod", 16, FontOption.Arial);
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
            FileBrowser.ShowSaveDialog(OnExportModSuccess, OnCancel, FileBrowser.PickMode.Folders, title: "选择要导出的位置（手机端请先点击左边的Browse才能浏览）");
        });
    }

    private void OnExportModSuccess(string[] paths) {
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
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部Mod文件，而非默认存档位置的文件", 16);
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

    #endregion
}

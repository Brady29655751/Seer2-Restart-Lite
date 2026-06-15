using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using FTRuntime;

public class TitleManager : Manager<TitleManager>
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private List<GameObject> backgroundGadgetObjectList;
    [SerializeField] private IButton startButton;
    [SerializeField] private IText versionDateText, versionStringText;
    [SerializeField] private GameObject toolBarObject, resourceOperateObject, modOperateObject, helpObject;
    [Header("Game Icon Entrance")]
    [SerializeField] private bool playGameIconEntrance = true;
    [SerializeField] private RectTransform gameIconEntranceTarget;
    [SerializeField] private float gameIconEntranceStartScaleX = 0.08f;
    [SerializeField] private float gameIconEntranceDuration = 0.45f;
    [SerializeField] private float gameIconEntranceDelay = 0.05f;
    [SerializeField] private AnimationCurve gameIconEntranceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool showGameIconScanLine = true;
    [SerializeField] private float gameIconScanLineWidth = 10f;
    [SerializeField] private Color gameIconScanLineColor = new Color(0.25f, 0.95f, 1f, 0.85f);
    [SerializeField] private bool debugGameIconEntrance = false;
    [Header("Toolbar Entrance")]
    [SerializeField] private bool playToolbarEntrance = true;
    [SerializeField] private RectTransform toolbarEntranceTarget;
    [SerializeField] private Vector2 toolbarEntranceOffset = new Vector2(0f, -36f);
    [SerializeField] private float toolbarEntranceDuration = 0.42f;
    [SerializeField] private float toolbarEntranceDelay = 0.16f;
    [SerializeField] private AnimationCurve toolbarEntranceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Header("Toolbar Button Hover")]
    [SerializeField] private bool enableToolbarButtonHover = true;
    [SerializeField] private float toolbarButtonHoverScale = 1.06f;
    [SerializeField] private float toolbarButtonHoverLerpSpeed = 12f;

    private Graphic gameIconGraphic;
    private Image gameIconImage;
    private GameObject gameIconEntranceObject;
    private Vector2 gameIconTargetPosition;
    private Vector3 gameIconTargetScale;
    private Color gameIconTargetColor = Color.white;
    private Image.Type gameIconOriginalImageType;
    private Image.FillMethod gameIconOriginalFillMethod;
    private int gameIconOriginalFillOrigin;
    private bool gameIconOriginalFillClockwise;
    private float gameIconOriginalFillAmount = 1f;
    private RectTransform gameIconScanLineRect;
    private Image gameIconScanLineImage;
    private RectTransform toolbarRect;
    private CanvasGroup toolbarCanvasGroup;
    private Vector2 toolbarTargetPosition;
    private float toolbarTargetAlpha = 1f;
    private bool toolbarOriginalInteractable = true;
    private bool toolbarOriginalBlocksRaycasts = true;

    protected override void Awake()
    {
        base.Awake();
        PrepareGameIconEntrance();
        PrepareToolbarEntrance();
        ConfigureToolbarButtonHover();
    }

    private void Start() {
        GetVersion();
        FileBrowser.AddQuickLink("默认存档位置", Application.persistentDataPath);
        SetBackgroundPageImage();
        PlayGameIconEntrance();
        PlayToolbarEntrance();
    }

    private void PrepareGameIconEntrance()
    {
        if (!playGameIconEntrance)
        {
            LogGameIconEntrance("Prepare skipped: playGameIconEntrance is false");
            return;
        }

        var target = GetGameIconEntranceTarget();
        if ((target == null) || !target.gameObject.activeInHierarchy)
        {
            LogGameIconEntrance($"Prepare skipped: target={(target == null ? "null" : target.name)}, active={(target != null && target.gameObject.activeInHierarchy)}");
            return;
        }

        gameIconEntranceObject = target.gameObject;
        gameIconImage = target.GetComponent<Image>();
        gameIconGraphic = gameIconImage != null ? gameIconImage : target.GetComponent<Graphic>();
        if (gameIconGraphic == null)
        {
            LogGameIconEntrance($"Prepare skipped: {target.name} has no Graphic component");
            return;
        }

        gameIconTargetPosition = target.anchoredPosition;
        gameIconTargetScale = target.localScale;
        gameIconTargetColor = gameIconGraphic.color;

        if (gameIconImage != null)
        {
            gameIconOriginalImageType = gameIconImage.type;
            gameIconOriginalFillMethod = gameIconImage.fillMethod;
            gameIconOriginalFillOrigin = gameIconImage.fillOrigin;
            gameIconOriginalFillClockwise = gameIconImage.fillClockwise;
            gameIconOriginalFillAmount = gameIconImage.fillAmount;

            gameIconImage.type = Image.Type.Filled;
            gameIconImage.fillMethod = Image.FillMethod.Horizontal;
            gameIconImage.fillOrigin = 0;
            gameIconImage.fillClockwise = true;
            gameIconImage.fillAmount = 0f;
        }

        target.anchoredPosition = gameIconTargetPosition;
        target.localScale = new Vector3(gameIconTargetScale.x * gameIconEntranceStartScaleX, gameIconTargetScale.y, gameIconTargetScale.z);
        SetGameIconAlpha(gameIconTargetColor.a);
        PrepareGameIconScanLine(target);
        LogGameIconEntrance($"Prepared target={target.name}, scale={target.localScale}, targetScale={gameIconTargetScale}, duration={gameIconEntranceDuration}, delay={gameIconEntranceDelay}, graphic={gameIconGraphic.GetType().Name}, image={(gameIconImage != null)}, active={target.gameObject.activeInHierarchy}");
    }

    private RectTransform GetGameIconEntranceTarget()
    {
        if (gameIconEntranceTarget != null)
            return gameIconEntranceTarget;

        foreach (var rect in FindObjectsOfType<RectTransform>(true))
        {
            if (rect.gameObject.activeInHierarchy && (rect.name == "Game Icon"))
                return rect;
        }

        return null;
    }

    private void PlayGameIconEntrance()
    {
        if (!playGameIconEntrance)
        {
            LogGameIconEntrance("Play skipped: playGameIconEntrance is false");
            return;
        }

        var target = GetGameIconEntranceTarget();
        if ((target == null) || (gameIconGraphic == null))
        {
            LogGameIconEntrance($"Play skipped: target={(target == null ? "null" : target.name)}, graphic={(gameIconGraphic == null ? "null" : gameIconGraphic.GetType().Name)}");
            return;
        }

        LogGameIconEntrance($"Play started: target={target.name}, currentPos={target.anchoredPosition}, currentAlpha={gameIconGraphic.color.a}");
        StartCoroutine(AnimateGameIconEntrance(target));
    }

    private IEnumerator AnimateGameIconEntrance(RectTransform target)
    {
        if (gameIconEntranceDelay > 0f)
        {
            LogGameIconEntrance($"Delay begin: {gameIconEntranceDelay}s");
            yield return new WaitForSecondsRealtime(gameIconEntranceDelay);
            LogGameIconEntrance("Delay end");
        }

        var duration = Mathf.Max(0.2f, gameIconEntranceDuration);
        if (duration <= 0f)
        {
            FinishGameIconEntrance(target);
            yield break;
        }

        var elapsed = 0f;
        var frame = 0;
        var startScale = new Vector3(gameIconTargetScale.x * gameIconEntranceStartScaleX, gameIconTargetScale.y, gameIconTargetScale.z);
        while (elapsed < duration)
        {
            var progress = Mathf.Clamp01(elapsed / duration);
            var easedProgress = EvaluateGameIconEntranceCurve(progress);
            ApplyGameIconReveal(target, startScale, easedProgress);
            var frameDelta = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            LogGameIconEntrance($"Frame {frame}: elapsed={elapsed:F3}, delta={Time.unscaledDeltaTime:F3}, clampedDelta={frameDelta:F3}, progress={progress:F3}, eased={easedProgress:F3}, fill={(gameIconImage != null ? gameIconImage.fillAmount : 1f):F3}, scale={target.localScale}, active={target.gameObject.activeInHierarchy}");
            elapsed += frameDelta;
            frame++;
            yield return null;
        }

        FinishGameIconEntrance(target);
    }

    private float EvaluateGameIconEntranceCurve(float progress)
    {
        if ((gameIconEntranceCurve != null) && (gameIconEntranceCurve.length >= 2))
            return gameIconEntranceCurve.Evaluate(progress);

        return progress * progress * (3f - 2f * progress);
    }

    private void FinishGameIconEntrance(RectTransform target)
    {
        target.anchoredPosition = gameIconTargetPosition;
        target.localScale = gameIconTargetScale;
        gameIconGraphic.color = gameIconTargetColor;
        if (gameIconImage != null)
        {
            gameIconImage.type = gameIconOriginalImageType;
            gameIconImage.fillMethod = gameIconOriginalFillMethod;
            gameIconImage.fillOrigin = gameIconOriginalFillOrigin;
            gameIconImage.fillClockwise = gameIconOriginalFillClockwise;
            gameIconImage.fillAmount = gameIconOriginalFillAmount;
        }

        if (gameIconScanLineImage != null)
        {
            gameIconScanLineImage.enabled = false;
        }

        LogGameIconEntrance($"Finished: finalPos={target.anchoredPosition}, finalAlpha={gameIconGraphic.color.a}, active={target.gameObject.activeInHierarchy}");
    }

    private void SetGameIconAlpha(float alpha)
    {
        var color = gameIconTargetColor;
        color.a = alpha;
        gameIconGraphic.color = color;
    }

    private void ApplyGameIconReveal(RectTransform target, Vector3 startScale, float progress)
    {
        target.anchoredPosition = gameIconTargetPosition;
        target.localScale = Vector3.LerpUnclamped(startScale, gameIconTargetScale, progress);

        if (gameIconImage != null)
        {
            gameIconImage.fillAmount = progress;
        }
        else
        {
            SetGameIconAlpha(Mathf.LerpUnclamped(0f, gameIconTargetColor.a, progress));
        }

        UpdateGameIconScanLine(target, progress);
    }

    private void PrepareGameIconScanLine(RectTransform target)
    {
        if (!showGameIconScanLine)
            return;

        var scanLineObject = new GameObject("Game Icon Scan Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        scanLineObject.transform.SetParent(target, false);

        gameIconScanLineRect = scanLineObject.GetComponent<RectTransform>();
        gameIconScanLineRect.anchorMin = new Vector2(0f, 0f);
        gameIconScanLineRect.anchorMax = new Vector2(0f, 1f);
        gameIconScanLineRect.pivot = new Vector2(0.5f, 0.5f);
        gameIconScanLineRect.sizeDelta = new Vector2(gameIconScanLineWidth, 0f);
        gameIconScanLineRect.anchoredPosition = Vector2.zero;

        gameIconScanLineImage = scanLineObject.GetComponent<Image>();
        gameIconScanLineImage.raycastTarget = false;
        gameIconScanLineImage.color = new Color(gameIconScanLineColor.r, gameIconScanLineColor.g, gameIconScanLineColor.b, 0f);
    }

    private void UpdateGameIconScanLine(RectTransform target, float progress)
    {
        if ((gameIconScanLineRect == null) || (gameIconScanLineImage == null))
            return;

        var rectWidth = target.rect.width;
        gameIconScanLineRect.anchoredPosition = new Vector2(rectWidth * progress, 0f);
        var alpha = gameIconScanLineColor.a * Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);
        gameIconScanLineImage.color = new Color(gameIconScanLineColor.r, gameIconScanLineColor.g, gameIconScanLineColor.b, alpha);
    }

    private void LogGameIconEntrance(string message)
    {
        if (!debugGameIconEntrance)
            return;

        Debug.Log($"[TitleManager][GameIconEntrance] {message}");
    }

    private void PrepareToolbarEntrance()
    {
        if (!playToolbarEntrance)
            return;

        toolbarRect = GetToolbarEntranceTarget();
        if ((toolbarRect == null) || !toolbarRect.gameObject.activeInHierarchy)
            return;

        toolbarCanvasGroup = toolbarRect.GetComponent<CanvasGroup>();
        if (toolbarCanvasGroup == null)
        {
            toolbarCanvasGroup = toolbarRect.gameObject.AddComponent<CanvasGroup>();
        }

        toolbarTargetPosition = toolbarRect.anchoredPosition;
        toolbarTargetAlpha = toolbarCanvasGroup.alpha;
        toolbarOriginalInteractable = toolbarCanvasGroup.interactable;
        toolbarOriginalBlocksRaycasts = toolbarCanvasGroup.blocksRaycasts;

        toolbarRect.anchoredPosition = toolbarTargetPosition + toolbarEntranceOffset;
        toolbarCanvasGroup.alpha = 0f;
        toolbarCanvasGroup.interactable = false;
        toolbarCanvasGroup.blocksRaycasts = false;
    }

    private RectTransform GetToolbarEntranceTarget()
    {
        if (toolbarEntranceTarget != null)
            return toolbarEntranceTarget;

        foreach (var rect in FindObjectsOfType<RectTransform>(true))
        {
            if (rect.gameObject.activeInHierarchy && (rect.name == "Start Toolbar"))
                return rect;
        }

        if (toolBarObject == null)
            return null;

        var current = toolBarObject.transform;
        while (current != null)
        {
            if (current.name == "Start Toolbar")
                return current.GetComponent<RectTransform>();

            current = current.parent;
        }

        return toolBarObject.GetComponent<RectTransform>();
    }

    private void PlayToolbarEntrance()
    {
        if (!playToolbarEntrance || (toolbarRect == null) || (toolbarCanvasGroup == null))
            return;

        StartCoroutine(AnimateToolbarEntrance());
    }

    private IEnumerator AnimateToolbarEntrance()
    {
        if (toolbarEntranceDelay > 0f)
            yield return new WaitForSecondsRealtime(toolbarEntranceDelay);

        var duration = Mathf.Max(0.2f, toolbarEntranceDuration);
        var elapsed = 0f;
        var startPosition = toolbarTargetPosition + toolbarEntranceOffset;
        while (elapsed < duration)
        {
            var progress = Mathf.Clamp01(elapsed / duration);
            var easedProgress = EvaluateToolbarEntranceCurve(progress);
            toolbarRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, toolbarTargetPosition, easedProgress);
            toolbarCanvasGroup.alpha = Mathf.LerpUnclamped(0f, toolbarTargetAlpha, easedProgress);
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            yield return null;
        }

        FinishToolbarEntrance();
    }

    private float EvaluateToolbarEntranceCurve(float progress)
    {
        if ((toolbarEntranceCurve != null) && (toolbarEntranceCurve.length >= 2))
            return toolbarEntranceCurve.Evaluate(progress);

        return progress * progress * (3f - 2f * progress);
    }

    private void FinishToolbarEntrance()
    {
        toolbarRect.anchoredPosition = toolbarTargetPosition;
        toolbarCanvasGroup.alpha = toolbarTargetAlpha;
        toolbarCanvasGroup.interactable = toolbarOriginalInteractable;
        toolbarCanvasGroup.blocksRaycasts = toolbarOriginalBlocksRaycasts;
    }

    private void ConfigureToolbarButtonHover()
    {
        if (!enableToolbarButtonHover)
            return;

        var toolContainer = GetToolbarToolContainer();
        if (toolContainer == null)
            return;

        foreach (Transform child in toolContainer)
        {
            if (!IsMainToolbarButtonName(child.name))
                continue;

            var feedback = child.GetComponent<TitleToolbarButtonHoverFeedback>();
            if (feedback == null)
            {
                feedback = child.gameObject.AddComponent<TitleToolbarButtonHoverFeedback>();
            }

            feedback.SetFeedback(toolbarButtonHoverScale, toolbarButtonHoverLerpSpeed);
        }
    }

    private Transform GetToolbarToolContainer()
    {
        var toolbar = GetToolbarEntranceTarget();
        if (toolbar == null)
            return null;

        foreach (var rect in toolbar.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == "Tool")
                return rect.transform;
        }

        return null;
    }

    private bool IsMainToolbarButtonName(string buttonName)
    {
        return (buttonName == "Resource") || (buttonName == "Mod") || (buttonName == "Help");
    }

    private void SetBackgroundPageImage() {
        var modAnim = ResourceManager.instance.GetMapAnimPrefab(0, "0-idle");
        if (modAnim == null)
        {
            void OnLoadBGMFinished(AudioClip modBGM)
            {
                var modBg = ResourceManager.instance.GetLocalAddressables<Sprite>("Activities/FirstPage", true);
                var mapRes = new MapResources(modBg, null, modBGM);
                var map = new Map(){ id = 0, resources = mapRes };
                SetBackground(map, true);
            }
            
            ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/FirstPage", true, OnLoadBGMFinished, (error) => OnLoadBGMFinished(null));    
            return;
        }

        ResourceManager.instance.LoadMap(0, SetBackground, (error) =>
        {
            Map map = new Map(){ id = 0 };
            ResourceManager.instance.LoadMapResources(map, SetBackground, (error) => SetBackground(map));
        });
    }

    private void SetBackground(Map map)
    {
        SetBackground(map, false);
    }

    private void SetBackground(Map map, bool hideGameIcon)
    {
        var res = map.resources;
        if (res.bgm != null)
            AudioSystem.instance.PlayMusic(res.bgm, AudioVolumeType.None); 

        if (res.bg != null)
        {
            backgroundImage.SetSprite(res.bg);
            backgroundImage.color = Color.white;
            HideBackgroundGadgets(hideGameIcon);
        }

        if (res.anim != null)
        {
            var obj = Instantiate(res.anim, Camera.main.transform);
            obj.transform.localScale = map.anim.AnimScale;
            obj.transform.position = map.anim.AnimPos;
            backgroundImage.gameObject.SetActive(false);
        }
    }

    private void HideBackgroundGadgets(bool hideGameIcon)
    {
        foreach (var obj in backgroundGadgetObjectList)
        {
            if (obj == null)
                continue;

            if (!hideGameIcon && ((obj == gameIconEntranceObject) || (obj.name == "Game Icon")))
                continue;
            
            obj.SetActive(false);
        }
    }

    public void GameStart() {
        if (!CheckVersion())
            return;
        
        SceneLoader.instance.ChangeScene(SceneId.Login);
    }

    public void GetVersion(bool onlineMode = false) {
        Hintbox hintbox = null;
        if (onlineMode) {
            hintbox = Hintbox.OpenHintboxWithContent("正在联网获取版本档案\n（没梯子会不稳定）", 16);
            hintbox.SetOptionNum(0);
        }
        GameManager.instance.onlineMode = onlineMode;
        GameManager.instance.GetVersionData(() => OnGetVersionSuccess(hintbox));
    }
    
    private void OnGetVersionSuccess(Hintbox onlineCheckHintbox = null) {
        var isVersionOK = CheckVersion();
        if (onlineCheckHintbox == null)
            return;

        if (!isVersionOK) {
            onlineCheckHintbox.ClosePanel();
            return;
        }

        var message = GameManager.instance.onlineMode ? "获取联网版本档案成功\n目前为最新版本" : "获取联网版本档案失败\n使用本地版本档案";
        onlineCheckHintbox.SetContent("<color=#ffbb33>" + message + "</color>", 16, FontOption.Arial);
        onlineCheckHintbox.SetOptionNum(1);
    }

    private bool CheckVersion() {
        if (GameManager.versionData == null) {
            RequestManager.OnRequestFail("正在获取版本档案，请稍候再试");
            return false;
        }

        if (GameManager.versionData.IsEmpty()) {
            versionDateText?.SetText("----/--/--");
            versionStringText?.SetText("----");
            RequestManager.OnRequestFail("获取版本档案失败，请重新启动游戏");
            return false;
        }

        versionDateText?.SetText(GameManager.versionData.releaseDate.ToString("yyyy/MM/dd"));
        versionStringText?.SetText(GameManager.versionData.gameVersion);
        return CheckBuildVersion() && CheckResourceVersion();
    }

    private bool CheckBuildVersion() {
        if (GameManager.versionData.buildVersion == Application.version)
            return true;

        startButton?.SetInteractable(false, false);
        OpenUpdateBuildHintbox(150 * (long)1_000_000);
        // RequestManager.OnRequestFail("检测到新版本，正在获取更新档案大小，请稍候");
        // RequestManager.instance.GetDownloadSize(GameManager.gameDownloadUrl, OpenUpdateBuildHintbox);
        return false;
    }

    private bool CheckResourceVersion() {
        var resourceVersion = SaveSystem.GetResourceVersion();
        if (VersionData.Compare(resourceVersion, GameManager.versionData.resourceVersion) >= 0)
            return true;

        /*
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("资源更新");
        hintbox.SetContent("检测到游戏资源需要更新\n是否联网下载游戏资源？\n（需要梯子并建议预留500MB以上空间）", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnDownloadBasicResources);
        hintbox.SetOptionCallback(OnResourceFail, false);
        */
        OnResourceFail();
        return false;
    }

    private void OnCancel() {}

    #region update

    private void OpenUpdateBuildHintbox(long size) {
        startButton?.SetInteractable(true, false);

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        if (size == -1) {                
            hintbox.SetContent("检测到新版本，但获取档案大小失败，请手动更新游戏", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }

        hintbox.SetContent("检测到新版本。点击确认选择下载位置。\n建议预留" + 
            (size / 1_000_000) + "MB 以上空间。", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnSelectDownloadPosition, true);
    }

    private void OnSelectDownloadPosition() {
        var downloadUrl = GameManager.gameDownloadUrl;
        var filter = new FileBrowser.Filter("Game", GameManager.gameFilePostfix);
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowSaveDialog(OnSelectDownloadPositionSuccess, OnCancel, 
            FileBrowser.PickMode.Files, initialFilename: "Seer2_Restart_Lite", 
            title: "选择新版本下载位置（手机端请先点击左边的Browse才能浏览）");
    }

    private void OnSelectDownloadPositionSuccess(string[] paths) {
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(0);
        var loadingText = loadingScreen?.loadingText;
        loadingText?.SetText("正在下载新版本");
        RequestManager.instance.Download(GameManager.gameDownloadUrl, paths[0], 
            () => loadingScreen?.SetText("下载完成，请关闭游戏并解压安装新游戏档案\n电脑的旧版本可自行删除，手机的请覆盖安装"),
            (error) => loadingScreen?.SetText("下载失败，请重新启动。错误如下：\n" + error),
            loadingScreen.ShowLoadingProgress
        );
    }

    #endregion

    #region resource

    public void OnDownloadBasicResources() {
        var zipPath = Application.persistentDataPath + "/BasicResources.zip";
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(text: "正在下载基础资源");

        RequestManager.instance.Download(GameManager.resourceUrl, zipPath, 
            OnUnzipBasicResources, error => 
            {
                SceneLoader.instance.HideLoadingScreen();
                OnResourceFail();
            }, 
            loadingScreen.ShowLoadingProgress);
    }

    public void OnDownloadPetAnimationResources() {
        var zipPath = Application.persistentDataPath + "/PetAnimation.zip";
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(text: "正在下载精灵动画资源");
        RequestManager.instance.Download(GameManager.petAnimationUrl, zipPath, 
            OnUnzipPetAnimationResources, error => 
            {
                SceneLoader.instance.HideLoadingScreen();
                OnResourceFail();
            }, 
            loadingScreen.ShowLoadingProgress);
    }

    public void OnOperateResources()
    {
        toolBarObject?.SetActive(false);
        resourceOperateObject?.SetActive(true);
    }

    public void OnUpdateResources()
    {
        FileBrowser.ShowLoadDialog((paths) => OnResourceSuccess(paths, true), OnCancel, FileBrowser.PickMode.FilesAndFolders, title: "选择要导入的资源（手机端请先点击左边的Browse才能浏览）");
    }

    public void OnImportResources()
    {
        FileBrowser.ShowLoadDialog((paths) => OnResourceSuccess(paths, false), OnCancel, FileBrowser.PickMode.FilesAndFolders, title: "选择要导入的资源（手机端请先点击左边的Browse才能浏览）");
    }

    private void OnResourceSuccess(string[] paths, bool overwrite) {
        if ((paths[0] == Application.persistentDataPath) || 
            FileBrowserHelpers.IsPathDescendantOfAnother(paths[0], Application.persistentDataPath)) {
            Hintbox.OpenHintboxWithContent("需选择外部文件，而非默认存档位置文件", 16);
            return;
        }

        string action = overwrite ? "更新" : "导入";

        if (paths[0].EndsWith(".zip"))
        {
            if (SaveSystem.TryGetZipFileEntry(paths[0], "Resources/", out var entry) && (entry != null))
            {
                bool result = SaveSystem.TryUnzipFile(paths[0], Application.persistentDataPath, "Resources", overwrite);
                string zipMessage = result ? (action + "<color=#ffbb33>【基础资源包】</color>成功！") : "解压<color=#ffbb33>【基础资源包】</color>发生错误，请尝试手动解压导入";
                Hintbox.OpenHintboxWithContent(zipMessage, 16);
                return;
            }

            if (SaveSystem.TryGetZipFileEntry(paths[0], "PetAnimation/", out entry) && (entry != null))
            {
                bool result = SaveSystem.TryUnzipFile(paths[0], Application.persistentDataPath, "PetAnimation", overwrite);
                string zipMessage = result ? (action + "<color=#ffbb33>【动画资源包】</color>成功！") : "解压<color=#ffbb33>【动画资源包】</color>发生错误，请尝试手动解压导入";
                Hintbox.OpenHintboxWithContent(zipMessage, 16);
                return;
            }

            Hintbox.OpenHintboxWithContent("读取压缩档发生错误", 16);
            return;
        }

        var isSuccessImporting = SaveSystem.TryImportResources(paths[0], out var error, overwrite);
        var message = action + (isSuccessImporting ? "成功" : ("失败\n" + error));
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);
    }

    public void OnResourceFail() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetSize(540, 360);
        hintbox.SetTitle("获取资源档案失败");
        hintbox.SetContent("请到群内下载「基础资源包」\n解压后点击右下方的「导入资源包」按钮\n手动导入里面名为Resources的文件夹\n\n" +
            "若要新增动画，同样到群内下载对应的「动画资源包」\n详情请查看群公告和右下方的<color=#ffbb33>【帮助】</color>\n\n" +
            "<color=#ffbb33>导入过程会卡顿，请耐心等待【导入成功】的提示出现</color>\n若长时间卡在地图加载页面，请重新正确导入", 16, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }


    private void OnUnzipBasicResources()
    {
        var zipPath = Application.persistentDataPath + "/BasicResources.zip";
        bool result = SaveSystem.TryUnzipFile(zipPath, Application.persistentDataPath, "Resources");

        FileBrowserHelpers.DeleteFile(zipPath);

        if (!result)
        {
            SceneLoader.instance.HideLoadingScreen();
            Hintbox.OpenHintboxWithContent("基础资源解压失败，请手动导入资源", 16);
            return;
        }

        OnDownloadPetAnimationResources();
    }

    private void OnUnzipPetAnimationResources() {
        var zipPath = Application.persistentDataPath + "/PetAnimation.zip";
        bool result = SaveSystem.TryUnzipFile(zipPath, Application.persistentDataPath, "PetAnimation");
        string message = result ? "全部资源下载完毕，请重新启动游戏" : "精灵动画资源解压失败，请手动导入资源";

        FileBrowserHelpers.DeleteFile(zipPath);
        SceneLoader.instance.HideLoadingScreen();
        var hintbox = Hintbox.OpenHintboxWithContent(message, 16);        
        if (result)
            hintbox.SetOptionCallback(Application.Quit);
    }

    #endregion

    # region mod

    public void OnOperateMod() {
        toolBarObject?.SetActive(false);
        modOperateObject?.SetActive(true);
    }

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

    #endregion

    #region help

    public void OnHelp(bool active) {
        helpObject?.SetActive(active);
    }

    #endregion
}

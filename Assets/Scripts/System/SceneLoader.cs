using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class SceneLoader : Singleton<SceneLoader>
{
    private const int sceneNum = 4;
    private LoadingScreen currentLoadingScreen;
    private int nextSceneId, lastSceneId;

    public GameObject canvas;
    public LoadingScreen defaultLoadingScreen, transparentLoadingScreen;
    public LoadingScreen[] loadingScreens = new LoadingScreen[sceneNum];

    public SceneId GetLastScene()
    {
        return (SceneId)lastSceneId;
    }

    public static bool IsCurrentSceneSameAsLast() {
        return instance.lastSceneId == instance.nextSceneId;
    }

    public void ChangeScene(SceneId index, bool network = false)
    {
        PhotonNetwork.AutomaticallySyncScene = network;
        lastSceneId = nextSceneId;
        nextSceneId = (int)index;
        OnBeforeLoading();
    }

    public LoadingScreen ShowLoadingScreen(int sceneId = 0, string text = null) {
        canvas.SetActive(true);
        for (int i = -1; i < loadingScreens.Length; i++) {
            GetLoadingScreen(i).gameObject.SetActive(false);
        }
        currentLoadingScreen = GetLoadingScreen(sceneId);
        currentLoadingScreen.gameObject.SetActive(true);
        currentLoadingScreen.background.gameObject.SetActive(true);

        if (text != null)
            currentLoadingScreen.SetText(text);
            
        return currentLoadingScreen;
    }
    
    public void HideLoadingScreen() {
        canvas.SetActive(false);   
    }

    private void OnBeforeLoading() {
        ShowLoadingScreen(nextSceneId);

        ResourceManager.instance.UnloadResources();
        Resources.UnloadUnusedAssets();

        currentLoadingScreen.OnBeforeLoading(OnLoading);
    }

    private void OnLoading() {
        currentLoadingScreen.OnLoading(OnAfterLoading, nextSceneId);
    }

    private void OnAfterLoading() {
        currentLoadingScreen.OnAfterLoading(OnFinishLoading);
    }

    private void OnFinishLoading() {
        HideLoadingScreen();
    }

    /*
    private IEnumerator ChangeSceneAsyncPhoton(SceneId index) {
        while (PhotonNetwork.LevelLoadingProgress < 1) {
            float progress = PhotonNetwork.LevelLoadingProgress;
            
            loadingSlider.value = progress;
            loadingText.text = (Mathf.CeilToInt(progress * 10000) / 100).ToString() + "%";
            yield return null;
        }

        loadingScreen.SetActive(false);
    }
    */

    private LoadingScreen GetLoadingScreen(int index) {
        if (index < 0)
            return transparentLoadingScreen;

        if (index >= loadingScreens.Length)
            return defaultLoadingScreen;
        
        return (loadingScreens[index] ?? defaultLoadingScreen);
    }
}

public enum SceneId
{
    Title = 0,
    Login = 1,
    Map = 2,
    Battle = 3,
    Room = 4,
    Game = 5,
}

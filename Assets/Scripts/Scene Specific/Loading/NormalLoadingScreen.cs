using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NormalLoadingScreen : LoadingScreen
{
    protected override IEnumerator ChangeSceneCoroutine(int sceneIndex, Action finishedCallback = null) {
        int loadMapSuccess = 0;
        var sceneId = (SceneId)sceneIndex;
        void OnRequestSuccess(Map map) {
            Player.instance.currentMap = map;
            loadMapSuccess = 1;
        }
        void OnRequestFail(string error) {
            loadingText?.SetText(error);
            loadMapSuccess = -1;
        }

        loadingText?.SetText("正在载入地图");
        Database.instance.GetMap(Player.instance.currentMapId, OnRequestSuccess, OnRequestFail);
        while (loadMapSuccess == 0) {
            yield return null;
        }

        if (loadMapSuccess == -1)
            yield break;

        loadingText?.SetText("正在载入场景资源");
        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone) {
            ShowLoadingProgress(operation.progress / 0.9f);
            yield return null;
        }

        GameManager.instance.ChangeState((sceneId == SceneId.Game) ? GameState.MiniGame : GameState.Play);

        if (loadMapSuccess == 1)
            finishedCallback?.Invoke();
    }

}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.AddressableAssets.ResourceLocators;

public class LoginLoadingScreen : LoadingScreen
{
    protected override IEnumerator ChangeSceneCoroutine(int sceneIndex, Action finishedCallback = null) {
        AssetBundle.UnloadAllAssetBundles(true);
        
        loadingText?.SetText("正在初始化");
        /*
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        loadingText?.SetText("正在检测更新");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (!IsAsyncHandleSucceeded(checkHandle, "检测更新失败"))
            yield break;

        if (checkHandle.Result.Count > 0) {
            loadingText?.SetText("正在获取更新目录");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateHandle;

            if (!IsAsyncHandleSucceeded(updateHandle, "获取更新目录失败"))
                yield break;

            var updateList = updateHandle.Result;
            var updateCount = updateList.Count;
            Addressables.Release(updateHandle);

            for (int i = 0; i < updateCount; i++) {
                IEnumerable<object> keys = new List<object>(updateList[i].Keys);
                var sizeHandle = Addressables.GetDownloadSizeAsync(keys);
                yield return sizeHandle;

                if (!IsAsyncHandleSucceeded(sizeHandle, "获取更新档案大小失败"))
                    yield break;

                var totalDownloadSize = sizeHandle.Result;
                if (totalDownloadSize > 0) {
                    var downloadHandle  = Addressables.DownloadDependenciesAsync(keys, true);
                    while (!downloadHandle.IsDone) {
                        if (downloadHandle.Status == AsyncOperationStatus.Failed) {
                            loadingText?.SetText("档案下载失败，请重新启动。\n错误：" + downloadHandle.OperationException);
                            yield break;
                        }
                        var status = downloadHandle.GetDownloadStatus();
                        float progress = downloadHandle.PercentComplete;
                        ShowLoadingProgress(progress / 100f);

                        var downloadProgressText = (status.DownloadedBytes / 1000) + " KB / " + (status.TotalBytes / 1000) + " KB";
                        var downloadCountText = "已下载：" + (i + 1) + " / " + updateCount;
                        loadingText?.SetText(downloadProgressText + "\n" + downloadCountText);
                        yield return null;
                    }
                }
            }
            loadingText?.SetText("更新完毕");
        }
        Addressables.Release(checkHandle);
        */
        loadingText?.SetText("正在获取服务器档案。若长时间没有回应，请重新启动。");
        while (!Database.instance.VerifyData(out string error))
            yield return null;
    

        loadingText?.SetText("正在载入场景资源");
        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone) {
            ShowLoadingProgress(operation.progress / 0.9f);
            yield return null;
        }

        finishedCallback?.Invoke();
    }

    private bool IsAsyncHandleSucceeded<T>(AsyncOperationHandle<T> asyncOperationHandle, string errorHeader) {
        if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
            loadingText?.SetText(errorHeader + "，请重新启动\n错误：" + asyncOperationHandle.OperationException);
            return false;
        }
        return true;
    }
}

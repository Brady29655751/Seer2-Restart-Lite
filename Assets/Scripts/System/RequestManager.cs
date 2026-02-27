using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Video;

public class RequestManager : Singleton<RequestManager>
{
    protected void Start() {
        // Addressables.LoadAssetAsync<TextAsset>("System/version").Completed += (handle) => {
        //     var versionData = ResourceManager.LoadXML<VersionData>(handle.Result);
        //     Debug.Log(versionData.versionId);
        // };
    }

    public static void OnRequestSuccess(string text) {
        Debug.Log(text);
    }

    public static void OnRequestFail(string text) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(text, 14, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }

    public void Get(string uri, Action<string> onSuccess = null, Action<string> onFail = null) {
        onSuccess ??= OnRequestSuccess;
        onFail ??= OnRequestFail;
        StartCoroutine(GetRequest(uri, onSuccess, onFail));
    }

    private IEnumerator GetRequest(string uri, Action<string> onSuccess, Action<string> onFail)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    onFail?.Invoke(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    onSuccess?.Invoke(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    public void GetDownloadSize(string uri, Action<long> onReceive = null) {
        StartCoroutine(GetDownloadSizeRequest(uri, onReceive));
    }

    private IEnumerator GetDownloadSizeRequest(string uri, Action<long> onReceive = null)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Head(uri)) 
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    onReceive?.Invoke(-1);
                    break;
                case UnityWebRequest.Result.Success:
                    string size = webRequest.GetResponseHeader("Content-Length");
                    onReceive?.Invoke(long.Parse(size));
                    break;
            }
        };
    }

    /// <summary>
    /// Download file form server.
    /// </summary>
    /// <param name="uri">File url</param>
    /// <param name="path">The path at disk that will download to</param>
    /// <param name="onSuccess">Action when success</param>
    /// <param name="onFail">Action when fail</param>
    /// <param name="onProgress">Action every frame when progress. Progress is in range [0, 1]</param>
    public void Download(string uri, string path, Action onSuccess = null, Action<string> onFail = null, Action<float> onProgress = null) {
        StartCoroutine(DownloadRequest(uri, path, onSuccess, onFail, onProgress));
    }

    private IEnumerator DownloadRequest(string uri, string path, Action onSuccess, Action<string> onFail, Action<float> onProgress)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            try {
                webRequest.downloadHandler = new DownloadHandlerFile(path);
            } catch (Exception e) {
                onFail?.Invoke(e.Message);
                yield break;
            }

            webRequest.SendWebRequest();

            while (!webRequest.isDone) {
                onProgress?.Invoke(webRequest.downloadProgress);
                yield return null;
            }

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    onFail?.Invoke(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    onSuccess?.Invoke();
                    break;
            }
        }
    }

    public void DownloadAudioClip(string uri, Action<AudioClip> onSuccess = null, Action<string> onFail = null, Action<float> onProgress = null) {
        StartCoroutine(DownloadAudioClipRequest(uri, onSuccess, onFail, onProgress));
    }

    private IEnumerator DownloadAudioClipRequest(string uri, Action<AudioClip> onSuccess = null, Action<string> onFail = null, Action<float> onProgress = null)
    {
        using (var webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            webRequest.SendWebRequest();

            while (!webRequest.isDone) {
                onProgress?.Invoke(webRequest.downloadProgress);
                yield return null;
            }

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    onFail?.Invoke(webRequest.error);
                    yield break;
                case UnityWebRequest.Result.Success:
                    break;
            }
    
            var clip = DownloadHandlerAudioClip.GetContent(webRequest);
            if (clip == null) {
                onFail?.Invoke("加载的音讯档案为空");
                yield break;
            }

            onSuccess?.Invoke(clip);
        }
    }

}

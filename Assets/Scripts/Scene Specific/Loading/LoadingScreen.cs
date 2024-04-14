using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : IMonoBehaviour
{
    public GameObject background;
    public Image[] loadingProgressNumbers = new Image[3];
    public Slider loadingSlider;
    public Text loadingText;

    public virtual void OnBeforeLoading(Action callback) {
        SetLoadingProgressNumber(0);
        callback?.Invoke();
    }

    public virtual void OnLoading(Action callback, int sceneIndex) {
        StartCoroutine(ChangeSceneCoroutine(sceneIndex, callback));
    }

    public virtual void OnAfterLoading(Action callback) {  
        callback?.Invoke();
    }

    protected virtual IEnumerator ChangeSceneCoroutine(int sceneIndex, Action finishedCallback = null) {
        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone) {
            ShowLoadingProgress(operation.progress / 0.9f);
            yield return null;
        }
        finishedCallback?.Invoke();
    }

    public virtual void ShowLoadingProgress(float progress) {
        int percent = Mathf.CeilToInt(progress * 100);

        if (loadingSlider != null)
            loadingSlider.value = progress;

        SetLoadingProgressNumber(percent);
    }

    protected void SetLoadingProgressNumber(int percent, string type = "Blue") {
        percent = Mathf.Clamp(percent, 0, 99);
        if ((loadingProgressNumbers != null) && (loadingProgressNumbers.Length >= 2)) {
            loadingProgressNumbers[0].sprite = ResourceManager.instance.GetSprite("Numbers/" + type + "/" + (percent / 10).ToString());
            loadingProgressNumbers[1].sprite = ResourceManager.instance.GetSprite("Numbers/" + type + "/" + (percent % 10).ToString());
        }
    }

    protected virtual IEnumerator WaitSecondsCoroutine(float seconds, Action finishedCallback = null) {
        yield return new WaitForSeconds(seconds);
        finishedCallback?.Invoke();
    }
}

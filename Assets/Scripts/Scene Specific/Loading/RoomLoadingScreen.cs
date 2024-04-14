using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

public class RoomLoadingScreen : LoadingScreen
{
    public override void OnBeforeLoading(Action callback) {
        AudioSystem.instance.StopMusic();
        AudioSystem.instance.StopEffect();
        
        SetLoadingProgressNumber(0);
        callback?.Invoke();
    }
}

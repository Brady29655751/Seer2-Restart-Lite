using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake() {

    }
    
    protected virtual void Start() {
        Init();
    }

    public virtual void Init() {

    }

    public IEnumerator WaitForSecondsCoroutine(float seconds, Action callback = null)
    {
        yield return new WaitForSeconds(seconds);

        callback?.Invoke();
    }
}

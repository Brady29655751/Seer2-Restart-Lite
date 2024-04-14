using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;

public abstract class Manager<T> : MonoBehaviour where T : MonoBehaviour 
{
    public static T instance {get; private set;}
    protected virtual void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
    }
    protected virtual void OnApplicationQuit() {
        instance = null;
        Destroy(gameObject);
    }
}

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
    public static T instance {get; private set;}
    protected virtual void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit() {
        instance = null;
        Destroy(gameObject);
    }
}

public abstract class PhotonSingleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks 
{
    public static T instance {get; private set;}
    protected virtual void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit() {
        instance = null;
        Destroy(gameObject);
    }
}

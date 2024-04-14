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
}

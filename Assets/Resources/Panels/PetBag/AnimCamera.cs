using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimCamera : Module
{
    [SerializeField] private Camera animCamera;
    [SerializeField] private RawImage rawImage;
    public Canvas canvas;

    protected override void Awake()
    {
        base.Awake();
        var renderTexture = RenderTexture.GetTemporary(1920, 1080);
        // renderTexture.Create();
        animCamera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
    }

    private void OnDestroy() 
    {
        RenderTexture.ReleaseTemporary(animCamera.targetTexture);    
    }


}


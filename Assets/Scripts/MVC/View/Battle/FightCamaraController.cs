using System;
using System.Collections;
using UnityEngine;

public class FightCamaraController : Module
{
    [SerializeField] private Transform mainCamaraTransform;
    [SerializeField] private RectTransform uiLayerRect; //通过调整相机位置实现屏幕震动
    [SerializeField] private Animator flashAnim;


    //以下是屏幕震动相关
    private const float ScreenShakeInitTime = 1.8f;
    private const float Interval = 0.1f;

    public void ScreenShake(float declineExtent = 0.6f, float shakeExtent = 0.75f) //衰减系数&震动幅度
    {
        StartCoroutine(ScreenShakeCoroutine(Interval, () =>
        {
            shakeExtent = -shakeExtent * declineExtent; //震动幅度逐渐减小
            mainCamaraTransform.position = new Vector3(shakeExtent, -shakeExtent / 3, -5);
            uiLayerRect.anchoredPosition = new Vector2(shakeExtent * 30, -shakeExtent * 10);
            ;
        }, () =>
        {
            mainCamaraTransform.position = new Vector3(0, 0, -5);
            uiLayerRect.anchoredPosition = new Vector2(0, 0);
        }));
    }

    private IEnumerator ScreenShakeCoroutine(float interval, Action shake, Action end)
    {
        float elapsedTime = 0f;
        while (elapsedTime < ScreenShakeInitTime)
        {
            shake();
            yield return new WaitForSeconds(interval);
            elapsedTime += interval;
        }

        end();
    }

    public void ScreenFlash()
    {
        flashAnim.SetTrigger("Flash");
    }
}
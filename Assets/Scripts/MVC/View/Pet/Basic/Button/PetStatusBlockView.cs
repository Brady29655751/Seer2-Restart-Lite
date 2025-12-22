using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetStatusBlockView : Module
{
    [SerializeField] private Sprite personalityUpSprite, personalityDownSprite;
    [SerializeField] private Image statusTypeImage, personalityBuffImage;
    [SerializeField] private Text statusText;
    [SerializeField] private Text evText;
    [SerializeField] private string evTextSuffix;
    [SerializeField] private IButton addButton;
    [SerializeField] private IButton minusButton;


    public override void Init()
    {
        base.Init();
        SetEVButtonHoldParam(1, 0.15f, true);
        SetEVButtonHoldParam(1, 0.15f, false);
    }

    private void SetEVButtonHoldParam(float threshold = -1, float delta = -1, bool positive = true) {
        IButton button = positive ? addButton : minusButton;
        button.holdThreshold = (threshold == -1) ? button.holdThreshold : threshold;
        button.holdSecondsDelta = (delta == -1) ? button.holdSecondsDelta : delta;
    }

    public void SetStatus(float status, int type) {
        statusTypeImage.SetSprite(Status.typeSprites[type]);
        statusText.text = status.ToString();
    }

    public void SetStatusColor(Color color) {
        statusText.color = color;
    }

    public void SetEV(float ev) {
        if (evText == null)
            return;

        evText.text = $"{(int)ev}{evTextSuffix}";
    }

    public void SetEVButtonsActive(bool active, bool positive = true) {
        IButton button = positive ? addButton : minusButton;
        button?.gameObject.SetActive(active);
    }

    public void SetPersonalityBuff(float status) {
        personalityBuffImage?.gameObject.SetActive(status != 0);
        personalityBuffImage?.SetSprite((status >= 0) ? personalityUpSprite : personalityDownSprite);
    }
}

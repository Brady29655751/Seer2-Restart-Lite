using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectNumView : Module
{
    [SerializeField] private IInputField ipf;
    [SerializeField] private Image icon;
    [SerializeField] private Text contentIndicatorText;
    [SerializeField] private Text numIndicatorText;


    public void SetValue(int value) {
        ipf.SetInputString(value.ToString());
    }

    public void SetIcon(Sprite sprite) {
        if (icon == null)
            return;

        icon.sprite = sprite;
    }

    public void SetContentIndicatorText(string text) {
        if (contentIndicatorText == null)
            return;

        contentIndicatorText.text = text;
    }

    public void SetNumIndicatorText(string text) {
        if (numIndicatorText == null)
            return;

        numIndicatorText.text = text;
    }

}

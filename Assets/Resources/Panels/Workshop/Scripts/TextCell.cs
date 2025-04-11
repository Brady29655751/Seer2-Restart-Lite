using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextCell : Module
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private IButton button;
    [SerializeField] private Text text;

    public void SetWidth(float width) {
        rectTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public void SetText(string content) {
        text?.SetText(content);
    }

    public void SetTextColor(Color color) {
        text?.SetColor(color);
    }

    public void SetFontSize(int fontsize) {
        if (text == null)
            return;

        text.fontSize = fontsize;
    }

    public void SetFontOption(FontOption font) {
        if (text == null)
            return;

        text.font = ResourceManager.instance.GetFont(font);
    }

    public void SetCallback(Action callback, string type = "click") {
        switch (type) {
            default:
                break;
            case "click":
                button?.onPointerClickEvent.SetListener(() => callback?.Invoke());
                break;
            case "over":
                button?.onPointerOverEvent.SetListener(() => callback?.Invoke());
                break;
            case "enter":
                button?.onPointerEnterEvent.SetListener(() => callback?.Invoke());
                break;
            case "exit":
                button?.onPointerExitEvent.SetListener(() => callback?.Invoke());
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsView : Module
{
    [SerializeField] private RectTransform scrollRect;
    [SerializeField] private IText contentText;

    public void SetContent(string content) {
        contentText?.SetText(content);
        contentText?.text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentText.size.y);
        scrollRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentText.size.y);
    }
}

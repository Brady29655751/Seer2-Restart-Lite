using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionBox : MonoBehaviour
{
    [SerializeField] private RectTransform boxRect;
    [SerializeField] private Text contentText;
    
    public void SetBoxPosition(Vector2 pos) {
        boxRect.anchoredPosition = pos;
    }

    public void SetBoxSize(Vector2 size) {
        boxRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        boxRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public void SetText(string _content) {
        contentText.text = _content;
    }
    
    
    
}

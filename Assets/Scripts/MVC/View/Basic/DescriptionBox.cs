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

    public void SetBoxSize(Vector2 size, bool preferredSizeX = false, bool preferredSizeY = false) 
    {   
        var x = Mathf.Max(size.x, preferredSizeX ? (contentText.preferredWidth + 10) : 0);
        var y = Mathf.Max(size.y, preferredSizeY ? (contentText.preferredHeight + 10) : 0);
        boxRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
        boxRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
    }

    public void SetText(string _content) {
        contentText.text = _content;
    }
    
    
    
}

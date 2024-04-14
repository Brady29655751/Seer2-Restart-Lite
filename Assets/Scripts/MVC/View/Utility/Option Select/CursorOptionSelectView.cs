using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorOptionSelectView : OptionSelectView
{
    [SerializeField] private Image cursorImage;
    [SerializeField] List<RectTransform> optionRects;

    public override void Select(int index)
    {
        base.Select(index);
        SetCursorImagePosition(index);
    }

    private void SetCursorImagePosition(int index) {
        if (!index.IsInRange(0, optionRects.Count))
            return;

        cursorImage.rectTransform.anchoredPosition = optionRects[index].anchoredPosition;
    }
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectModel : SelectModel<Pet>
{

    public  Vector2 startDragPos { get; private set; }
    public int startDragIndex { get; private set; } = -1;

    protected override void SetSelections(Pet[] selections)
    {
        Array.Resize(ref selections, capacity);
        base.SetSelections(selections);
    }

    public void StartDrag(int index, RectTransform rectTransform) {
        startDragIndex = index;
        startDragPos = rectTransform.anchoredPosition;
    }

    public void EndDrag(int index, RectTransform rectTransform) {
        rectTransform.anchoredPosition = startDragPos;

        startDragIndex = -1;
        startDragPos = new Vector2();
    }

    public void Drop(int index) {
        if ((selections[startDragIndex] == null) || (selections[index] == null))
            return;

        Swap(selections[startDragIndex], index);
        SetPage(page);
    }
}

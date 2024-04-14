using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollListView<T> : Module
{
    [SerializeField] private float prefabHeight;
    [SerializeField] private GameObject prefab;
    [SerializeField] private RectTransform scrollContentRect;

    public void SetStorage(List<T> storage, Action<int> callback = null) {
        int count = storage.Count;
        scrollContentRect.DestoryChildren();
        scrollContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15 + (prefabHeight + 5) * count);

        for (int i = 0; i < count; i++) {
            int copy = i;
            GameObject obj = Instantiate(prefab, scrollContentRect);
            RectTransform rect = obj.GetComponent<RectTransform>();
            IButton button = obj.GetComponent<IButton>();
            Text text = obj.GetComponentInChildren<Text>();

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, prefabHeight);
            rect.anchoredPosition = new Vector2(0, -10 - (prefabHeight + 5) * copy);
            button?.onPointerClickEvent?.SetListener(() => callback?.Invoke(copy));
            text?.SetText(GetTitle(storage[i]));
        }
    }

    protected virtual string GetTitle(T obj) {
        return string.Empty;
    }

}

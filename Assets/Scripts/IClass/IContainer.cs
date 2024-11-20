using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class IContainer : IMonoBehaviour, IDropHandler
{
    [SerializeField] public int index = 0;
    [SerializeField] protected RectTransform rectTransform;

    /// <summary>
    /// The invoked index is this container.
    /// The invoked rectTransform is the dragged one, not this one.
    /// </summary>
    [SerializeField] public UnityEvent<int, RectTransform> onDropEvent = new UnityEvent<int, RectTransform>();

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null) {
            var dragRect = eventData.pointerDrag.GetComponent<RectTransform>();
            dragRect.anchoredPosition = rectTransform.anchoredPosition;
            onDropEvent?.Invoke(index, dragRect);
        }
    }
}

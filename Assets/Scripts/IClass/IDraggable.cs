using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class IDraggable : IMonoBehaviour, IPointerDownHandler, IBeginDragHandler, 
    IEndDragHandler, IDragHandler
{
    public Canvas canvas;
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;

    protected override void Awake() {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public virtual void OnPointerDown(PointerEventData eventData) {

    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = true;
    }
}

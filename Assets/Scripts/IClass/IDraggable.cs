using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class IDraggable : IMonoBehaviour, IPointerDownHandler, IBeginDragHandler, 
    IEndDragHandler, IDragHandler
{
    [SerializeField] public bool isMovable = true;
    [SerializeField] public int index = 0;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] public UnityEvent<int, RectTransform> onBeginDragEvent = new UnityEvent<int, RectTransform>();
    [SerializeField] public UnityEvent<int, RectTransform> onDragEvent = new UnityEvent<int, RectTransform>();
    [SerializeField] public UnityEvent<int, RectTransform> onEndDragEvent = new UnityEvent<int, RectTransform>();

    protected Canvas canvas;

    protected override void Awake() {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    private void OnDestroy() {
        onBeginDragEvent?.RemoveAllListeners();
        onDragEvent?.RemoveAllListeners();
        onEndDragEvent?.RemoveAllListeners();
    }

    public virtual void OnPointerDown(PointerEventData eventData) {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        if (isMovable)
            canvasGroup.blocksRaycasts = false;

        onBeginDragEvent?.Invoke(index, rectTransform);
    }

    public virtual void OnDrag(PointerEventData eventData) {
        if (isMovable)
            rectTransform.anchoredPosition += eventData.delta / (canvas.scaleFactor * transform.parent.localScale);

        onDragEvent?.Invoke(index, rectTransform);
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        if (isMovable)
            canvasGroup.blocksRaycasts = true;
            
        onEndDragEvent?.Invoke(index, rectTransform);
    }

    public void SetEnable(bool enable) {
        this.enabled = enable;
    }
}

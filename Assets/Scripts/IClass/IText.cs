using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class IText : IMonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text { get; protected set; }
    public Color color => text.color;
    public Vector2 size => GetSize();
    [SerializeField] public UnityEvent<string> onPointerClickEvent = new UnityEvent<string>();
    [SerializeField] public UnityEvent<string> onPointerEnterEvent = new UnityEvent<string>();
    [SerializeField] public UnityEvent<string> onPointerExitEvent = new UnityEvent<string>();

    protected override void Awake()
    {
        base.Awake();
        text = gameObject.GetComponent<TMP_Text>(); 
    }

    private void OnDestroy() {
        onPointerClickEvent?.RemoveAllListeners();
        onPointerEnterEvent?.RemoveAllListeners();
        onPointerExitEvent?.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnterEvent?.Invoke(text.text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExitEvent?.Invoke(text.text);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClickEvent?.Invoke(text.text);
    }

    public Vector2 GetSize() {
        return new Vector2(text.preferredWidth, text.preferredHeight);
    }

    public void SetText(string text) {
        this.text.text = text;
    }

    public void SetColor(Color32 color) {
        text.color = color;
    }

    public void SetSize(int size) {
        text.fontSize = size;
    }

}

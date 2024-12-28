using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattlePetBuffBlockView : Module
{
    public Buff buff { get; private set; }
    public RectTransform rectTransform => rect;
    [SerializeField] private RectTransform rect;
    [SerializeField] private IButton button;
    [SerializeField] private Text valueText;
    
    public void SetBuff(Buff _buff, UnityAction onPointerEnter = null, UnityAction onPointerExit = null, UnityAction onPointerOver = null, UnityAction onPointerClick = null) {
        buff = _buff;
        button?.SetInteractable(buff != null);

        SetSprite();
        SetValue();
        SetActions(onPointerEnter, onPointerExit, onPointerOver, onPointerClick);
    }

    public void SetSprite() {
        button.SetSprite(buff?.info?.icon ?? SpriteSet.GetDefaultIconSprite(true));
    }

    public void SetSprite(Sprite sprite) {
        button.SetSprite(sprite ?? SpriteSet.GetDefaultIconSprite(true));
    }

    public void SetValue() {
        var value = buff?.value ?? 0;
        valueText.gameObject.SetActive(value != 0);
        valueText.text = value.ToString();
    }

    private void SetActions(UnityAction onPointerEnter, UnityAction onPointerExit, UnityAction onPointerOver, UnityAction onPointerClick) {
        button.onPointerEnterEvent.SetListener(onPointerEnter);
        button.onPointerOverEvent.SetListener(onPointerOver);
        button.onPointerExitEvent.SetListener(onPointerExit);
        button.onPointerClickEvent.SetListener(onPointerClick);
    }   
}

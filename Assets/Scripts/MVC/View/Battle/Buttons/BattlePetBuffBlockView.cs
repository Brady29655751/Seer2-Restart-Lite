using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattlePetBuffBlockView : Module
{
    public Buff buff { get; private set; }
    [SerializeField] private IButton button;
    [SerializeField] private Text valueText;
    
    public void SetBuff(Buff _buff, UnityAction onPointerEnter = null, UnityAction onPointerExit = null, UnityAction onPointerOver = null) {
        buff = _buff;
        SetSprite();
        SetValue();
        SetActions(onPointerEnter, onPointerExit, onPointerOver);
    }

    public void SetSprite() {
        button.SetSprite(  buff.info.icon);
    }

    public void SetValue() {
        valueText.gameObject.SetActive(buff.value != 0);
        valueText.text = buff.value.ToString();
    }

    private void SetActions(UnityAction onPointerEnter, UnityAction onPointerExit, UnityAction onPointerOver) {
        button.onPointerEnterEvent.SetListener(onPointerEnter);
        button.onPointerOverEvent.SetListener(onPointerOver);
        button.onPointerExitEvent.SetListener(onPointerExit);
    }   
}

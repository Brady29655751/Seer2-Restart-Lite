using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameView : Module
{
    [SerializeField] private Sprite edit, mark;
    [SerializeField] private Text nameText;
    [SerializeField] private IInputField ipf;
    [SerializeField] private IButton button;

    public void SetInputFieldActive(bool active) {
        ipf.gameObject.SetActive(active);
    }

    public void SetInputString(string text) {
        ipf.SetInputString(text);
    }

    public void SetPlaceHolderText(string text) {
        ipf.SetPlaceHolderText(text);
    }

    public void SetNameTextActive(bool active) {
        nameText.gameObject.SetActive(active);
    }

    public void SetNameText(string text) {
        nameText.text = text;
    }

    public void SetEditButtonSprite(bool isEditIcon) {
        Sprite icon = (isEditIcon ? edit : mark);
        button.SetSprite(icon);
    }

    public void OnAfterChangeName(bool isDone) {
        SetInputFieldActive(!isDone);
        SetPlaceHolderText(nameText.text);
        SetNameTextActive(isDone);
        SetEditButtonSprite(isDone);
    }

}

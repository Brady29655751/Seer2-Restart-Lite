using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementFilterView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private IButton controlButton;
    [SerializeField] private RectTransform background;

    public void OnControlButtonClick() {
        SetActive(!background.gameObject.activeSelf);
    }

    public void SetActive(bool active) {
        background.gameObject.SetActive(active);
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetElementInfo(Element element) {
        string text = element.ToString();
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleCenter);
    }

}

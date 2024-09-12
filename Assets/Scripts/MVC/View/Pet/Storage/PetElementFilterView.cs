using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementFilterView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private IButton controlButton;
    [SerializeField] private RectTransform background;
    [SerializeField] private List<PetElementFilterButton> elementButtonList;

    public override void Init() {
        // SetPage(0);
    }

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
        string text = element.GetElementName();
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleCenter);
    }

    public void SetPage(int page) {
        var offset = page * elementButtonList.Count;
        for (int i = 0; i < elementButtonList.Count; i++) {
            var elementId = offset + i;
            elementButtonList[i].gameObject.SetActive(elementId < PetElementSystem.elementNum);
            if (elementId >= PetElementSystem.elementNum)
                continue;

            elementButtonList[i].SetElement((Element)elementId);
        }
    }

}

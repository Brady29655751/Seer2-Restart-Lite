using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModule : Module
{
    [SerializeField] protected InfoPrompt infoPrompt;
    [SerializeField] protected DescriptionBox descriptionBox;

    public void SetInfoPromptActive(bool active) {
        infoPrompt?.SetActive(active);
    }

    public void SetDescriptionBoxActive(bool active) {
        descriptionBox?.gameObject.SetActive(active);
    }
}

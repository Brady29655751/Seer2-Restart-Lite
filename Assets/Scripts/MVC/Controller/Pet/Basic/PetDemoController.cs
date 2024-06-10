using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDemoController : Module
{
    [SerializeField] private PetDemoModel demoModel;
    [SerializeField] private PetDemoView demoView;

    public void SetPet(Pet pet) {
        demoModel.SetPet(pet);
        demoView.SetPet(pet);
    }

    public void SetInfoPromptActive(bool active) {
        demoView.SetInfoPromptActive(active);
    }

    public void SetElementInfoPromptContent() {
        demoView.SetElementInfoPromptContent(demoModel.element);
    }

    public void SetSubElementInfoPromptContent() {
        demoView.SetElementInfoPromptContent(demoModel.subElement);
    }

    public void SetFeatureInfoPromptContent() {
        demoView.SetFeatureInfoPromptContent(demoModel.feature);
    }

    public void SetEmblemInfoPromptContent(int index) {
        demoView.SetEmblemInfoPromptContent(demoModel.emblem, index);
    }

    public void SetIVInfoPromptContent() {
        demoView.SetIVInfoPromptContent(demoModel.iv);
    }
}

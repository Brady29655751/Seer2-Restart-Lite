using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDemoController : Module
{
    [SerializeField] private PetBagMode mode;
    [SerializeField] private PetDemoModel demoModel;
    [SerializeField] private PetDemoView demoView;

    public void SetPet(Pet pet)
    {
        demoModel.SetPet(pet);
        demoView.SetPet(pet, demoModel.animMode);
        if (mode == PetBagMode.YiTeRogue)
            demoView.SetIVRank((IVRanking)Mathf.Clamp(pet.talent.IVRankId, 0, 4));
    }

    public void SetMode(PetBagMode mode)
    {
        this.mode = mode;
    }

    public void SetPetAnimationActive(bool active)
    {
        demoView.SetAnimation(active ? demoModel.currentPet : null, demoModel.animMode);
    }

    public void TogglePetAnimationMode() {
        demoModel.SetAnimMode(!demoModel.animMode);
        SetPetAnimationActive(true);
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

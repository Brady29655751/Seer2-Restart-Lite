using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictInfoController : Module
{
    [SerializeField] private PetDictInfoModel infoModel;
    [SerializeField] private PetDictInfoView infoView;
    [SerializeField] private PetDemoController demoController;

    private Action<PetInfo> onEditPetCallback;

    public void SetPet(Pet pet) {
        infoModel.SetPetId((pet == null) ? 0 : pet.id);
        infoView.SetPetInfo(infoModel.petInfo);
    }

    public void SetEditPetCallback(Action<PetInfo> callback) {
        onEditPetCallback = callback;
    }

    public void SelectMode(PetDictionaryMode mode) {
        infoView.SelectMode(mode);
    }

    public void Link() {
        var info = infoModel.petInfo;
        if (info.basic.linkId == "Workshop") {
            Pet.Add(Pet.GetExamplePet(info.id));
            Hintbox.OpenHintboxWithContent("获得了 " + info.name + " ！", 16);
            return;
        }

        Panel.Link(info.basic.linkId);
    }

    public void SetInfoPromptActive(bool active) {
        infoView.SetInfoPromptActive(active);
    }

    public void SetHabitatInfoPromptContent() {
        infoView.SetHabitatInfoPromptContent(infoModel.petInfo.basic.habitat);
    }

    public void OpenExamplePetBagPanel() {
        infoView.OpenExamplePetBagPanel(infoModel.petInfo.id, () => {
            demoController?.SetPetAnimationActive(false);
        });
    }

    public void OnEditPet() {
        onEditPetCallback?.Invoke(infoModel.petInfo);
    }

}

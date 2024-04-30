using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictInfoController : Module
{
    [SerializeField] private PetDictInfoModel infoModel;
    [SerializeField] private PetDictInfoView infoView;

    public void SetPet(Pet pet) {
        infoModel.SetPetId((pet == null) ? 0 : pet.id);
        infoView.SetPetInfo(infoModel.petInfo);
    }

    public void Link() {
        Panel.Link(infoModel.petInfo.basic.linkId);
    }

    public void SetInfoPromptActive(bool active) {
        infoView.SetInfoPromptActive(active);
    }

    public void SetHabitatInfoPromptContent() {
        infoView.SetHabitatInfoPromptContent(infoModel.petInfo.basic.habitat);
    }

    public void OpenExamplePetBagPanel() {
        infoView.OpenExamplePetBagPanel(infoModel.petInfo.id);
    }

}

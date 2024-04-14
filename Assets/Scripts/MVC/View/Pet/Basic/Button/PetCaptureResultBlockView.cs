using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCaptureResultBlockView : Module
{
    [SerializeField] private PetSelectBlockView selectBlockView;
    [SerializeField] private PetInfoView basicInfoView;
    [SerializeField] private PetDictInfoView dictInfoView;

    public void SetPet(Pet pet) {
        selectBlockView?.SetPet(pet);
        basicInfoView?.SetPet(pet);
        dictInfoView?.SetDescription(pet.info.basic.description);
        dictInfoView?.SetBaseStatus(pet.normalStatus);
    }
}

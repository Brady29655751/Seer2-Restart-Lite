using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetInfoController : Module
{
    [SerializeField] private PetInfoModel infoModel;
    [SerializeField] private PetInfoView infoView;

    public void SetPet(Pet pet) {
        infoModel.SetPet(pet);
        infoView.SetPet(pet);
    }
}

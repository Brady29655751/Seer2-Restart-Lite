using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDetailController : Module
{
    [SerializeField] private PetStatusController statusController;
    [SerializeField] private PetCurrentSkillController skillController;
    [SerializeField] private PetInfoController infoController;

    public void SetPet(Pet pet) {
        statusController?.SetPet(pet);
        skillController?.SetPet(pet);
        infoController?.SetPet(pet);
    }

}

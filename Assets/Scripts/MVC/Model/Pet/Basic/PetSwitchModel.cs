using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSwitchModel : Module
{
    public Pet currentPet { get; private set; }

    public void SetPet(Pet pet) {
        currentPet = pet;
    }
}

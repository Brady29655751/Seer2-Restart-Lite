using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetSecretSkillModel : SelectModel<LearnSkillInfo>
{
    public Pet currentPet { get; private set; }

    public void SetPet(Pet pet) {
        currentPet = pet;
        if (pet == null)
            return;

        SetStorage(currentPet?.skills.secretSkillInfo.ToList());
    }
}

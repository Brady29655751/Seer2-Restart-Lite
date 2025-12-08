using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetSecretSkillModel : Module
{
    public Pet currentPet { get; private set; }
    public LearnSkillInfo[] secretSkillInfos => currentPet.skills.secretSkillInfo;
    public void SetPet(Pet pet) {
        currentPet = pet;
    }
}

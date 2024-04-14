using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretSkillPanel : Panel
{
    [SerializeField] private PetSecretSkillController secretSkillController;
    public void SetPet(Pet pet) {
        secretSkillController.SetPet(pet);
    }
}

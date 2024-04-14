using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSecretSkillController : Module
{
    [SerializeField] private PetSecretSkillModel secretSkillModel;
    [SerializeField] private PetSecretSkillView secretSkillView;

    public void SetPet(Pet pet) {
        secretSkillModel.SetPet(pet);
        secretSkillView.SetSecretSkillInfo(secretSkillModel.secretSkillInfos);
    }

    public void SetInfoPromptActive(bool active) {
        secretSkillView.SetInfoPromptActive(active);
    }

    public void SetSecretSkillInfo(int index) {
        if (!index.IsInRange(0, secretSkillModel.secretSkillInfos.Length)) {
            SetInfoPromptActive(false);
            return;
        }
        secretSkillView.SetSkillInfoPromptContent(secretSkillModel.secretSkillInfos[index].skill);
    }

}

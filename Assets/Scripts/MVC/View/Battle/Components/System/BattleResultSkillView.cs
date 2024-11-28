using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleResultSkillView : BattleBaseView
{
    [SerializeField] private PetSelectBlockView petBlock;
    [SerializeField] private List<BattlePetSkillBlockView> skillBlocks;

    public void ShowNewSkill(Pet pet, List<Skill> newSkills) {
        petBlock.SetPet(pet);
        for (int i = 0; i < skillBlocks.Count; i++)
            skillBlocks[i].SetSkill((i < newSkills.Count) ? newSkills[i] : null);
    }
}

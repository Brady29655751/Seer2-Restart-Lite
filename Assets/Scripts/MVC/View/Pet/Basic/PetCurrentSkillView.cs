using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCurrentSkillView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private IButton resetButton;
    [SerializeField] private PetSkillBlockView superSkillBlockView;
    [SerializeField] private PetSkillBlockView[] normalSkillBlockViews = new PetSkillBlockView[4];
    
    public void SetPet(Pet pet, BattleRule rule = BattleRule.Anger) {
        if (pet == null)
            return;
        
        SetNormalSkills(pet.skills.normalSkillInfo, rule);
        SetSuperSkill(pet.skills.superSkillInfo, rule);
    }

    public void SetNormalSkills(LearnSkillInfo[] normalSkillInfos, BattleRule rule = BattleRule.Anger) {
        Array.Resize(ref normalSkillInfos, 4);

        for (int i = 0; i < 4; i++) {
            normalSkillBlockViews[i].SetSkill(normalSkillInfos[i], rule);
        }
    }

    public void SetSuperSkill(LearnSkillInfo superSkillInfo, BattleRule rule = BattleRule.Anger) {
        superSkillBlockView.SetSkill(superSkillInfo, rule);
    }

    public void SelectNormalSkill(int index) {
        if (!index.IsInRange(0, 4))
            return;
        
        for (int i = 0; i < 4; i++) {
            normalSkillBlockViews[i].SetChosen(i == index);
        }
    }

    public void SelectSuperSkill(bool chosen) {
        superSkillBlockView.SetChosen(chosen);
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetSkillInfoPromptContent(Skill skill, bool showAtRight = true) {
        if (skill == null) {
            infoPrompt.SetActive(false);
            return;
        }

        infoPrompt.SetSkill(skill, showAtRight);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCurrentSkillController : Module
{
    [SerializeField] private PetCurrentSkillModel currentSkillModel;
    [SerializeField] private PetCurrentSkillView currentSkillView;

    public event Action<Skill> onSelectNormalSkillEvent;
    public event Action<Skill> onSelectSuperSkillEvent;

    public void SetPet(Pet pet) {
        currentSkillModel.SetPet(pet);
        currentSkillView.SetPet(pet);
    }

    public void OpenSecretSkillPanel() {
        if (currentSkillModel.currentPet.level < 60) {
            Hintbox hintbox = Hintbox.OpenHintbox();
            hintbox.SetTitle("提示");
            hintbox.SetContent("精灵达到60级才能学习隐藏技能", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        if (currentSkillModel.currentPet.skills.secretSkillInfo.Length <= 0) {
            Hintbox hintbox = Hintbox.OpenHintbox();
            hintbox.SetTitle("提示");
            hintbox.SetContent("该精灵没有隐藏技能", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        SecretSkillPanel secretSkillPanel = Panel.OpenPanel<SecretSkillPanel>();
        secretSkillPanel.SetPet(currentSkillModel.currentPet);
    }

    public void SelectNormalSkill(int index) {
        currentSkillModel.SelectNormalSkill(index);
        currentSkillView.SelectNormalSkill(index);
        onSelectNormalSkillEvent?.Invoke(currentSkillModel.currentSelectedNormalSkill);
    }

    public void SelectSuperSkill() {
        currentSkillModel.SelectSuperSkill();
        currentSkillView.SelectSuperSkill();
        onSelectSuperSkillEvent?.Invoke(currentSkillModel.currentSelectedSuperSkill);
    }

    public void SetInfoPromptActive(bool active) {
        currentSkillView.SetInfoPromptActive(active);
    }

    public void SetNormalSkillInfo(int index) {
        if (!index.IsInRange(0, currentSkillModel.normalSkills.Length)) {
            SetInfoPromptActive(false);
            return;
        }
        currentSkillView.SetSkillInfoPromptContent(currentSkillModel.normalSkills[index]);
    }

    public void SetSuperSkillInfo() {
        currentSkillView.SetSkillInfoPromptContent(currentSkillModel.superSkill);
    }

    public Skill SwapNormalSkill(Skill backupSkill) {
        if (!currentSkillModel.isNormalSkillChosen)
            return null;

        Skill currentSkill = currentSkillModel.currentSelectedNormalSkill;
        if ((currentSkill == null) || (backupSkill == null))
            return null;
        
        currentSkillModel.SwapPetNormalSkill(backupSkill);
        currentSkillView.SetNormalSkills(currentSkillModel.normalSkillInfos);
        return currentSkill;
    }

    public Skill SwapSuperSkill(Skill backupSkill) {
        if (!currentSkillModel.isSuperSkillChosen)
            return null;
        
        Skill currentSkill = currentSkillModel.currentSelectedSuperSkill;
        if ((currentSkill == null) || (backupSkill == null))
            return null;

        currentSkillModel.SwapPetSuperSkill(backupSkill);
        currentSkillView.SetSuperSkill(currentSkillModel.superSkillInfo);
        return currentSkill;
    }
}

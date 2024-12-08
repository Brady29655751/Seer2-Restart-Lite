using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBackupSkillController : Module
{
    [SerializeField] private PetBackupSkillModel backupSkillModel;
    [SerializeField] private PetBackupSkillView backupSkillView;
    [SerializeField] private PageView backupPageView;

    public event Action<Skill> onSelectNormalSkillEvent;
    public event Action<Skill> onSelectSuperSkillEvent;

    public Skill currentSelectedNormalSkill => backupSkillModel.currentSelectedNormalSkill;
    public Skill currentSelectedSuperSkill => backupSkillModel.currentSelectedSuperSkill;

    public bool Filter(Skill skill) => backupSkillModel.SpecialFilter(skill);

    public void SetPet(Pet pet) {
        backupSkillModel.SetPet(pet);
        backupSkillView.SetPet(backupSkillModel.normalSkillInfos, backupSkillModel.superSkillInfo);
        OnBackupSkillSetPage();
    }

    public void Refresh() {
        backupSkillModel.SetPet(backupSkillModel.currentPet, backupSkillModel.page, true);
        backupSkillView.SetPet(backupSkillModel.normalSkillInfos, backupSkillModel.superSkillInfo);
        OnBackupSkillSetPage();
    }

    public void SelectNormalSkill(int index) {
        backupSkillModel.SelectNormalSkill(index);
        backupSkillView.SelectNormalSkill(index);

        if (index < 0)
            return;

        onSelectNormalSkillEvent?.Invoke(backupSkillModel.currentSelectedNormalSkill);
    }

    public void SelectSuperSkill() {
        backupSkillModel.SelectSuperSkill();
        backupSkillView.SelectSuperSkill(backupSkillModel.isSuperSkillChosen);
        onSelectSuperSkillEvent?.Invoke(backupSkillModel.currentSelectedSuperSkill);
    }

    public void SetInfoPromptActive(bool active) {
        backupSkillView.SetInfoPromptActive(active);
    }

    public void SetNormalSkillInfo(int index) {
        if (!index.IsInRange(0, backupSkillModel.normalSkills.Length)) {
            SetInfoPromptActive(false);
            return;
        }
        backupSkillView.SetSkillInfoPromptContent(backupSkillModel.normalSkills[index]);
    }

    public void SetSuperSkillInfo() {
        backupSkillView.SetSkillInfoPromptContent(backupSkillModel.superSkill, false);
    }

    public Skill SwapNormalSkill(Skill currentSkill) {
        if (/*(!backupSkillModel.activeSelf) || */(!backupSkillModel.isNormalSkillChosen))
            return null;

        Skill backupSkill = backupSkillModel.currentSelectedNormalSkill;
        if ((currentSkill == null) || (backupSkill == null))
            return null;

        backupSkillModel.SwapPetNormalSkill(currentSkill);
        backupSkillView.SetNormalSkills(backupSkillModel.normalSkillInfos);
        return backupSkill;
    }

    public Skill SwapSuperSkill(Skill currentSkill) {
        if (/*(!backupSkillModel.activeSelf) || */(!backupSkillModel.isSuperSkillChosen))
            return null;

        Skill backupSkill = backupSkillModel.currentSelectedSuperSkill;
        if ((currentSkill == null) || (backupSkill == null))
            return null;
        
        backupSkillModel.SwapPetSuperSkill(currentSkill);
        backupSkillView.SetSuperSkill(backupSkillModel.superSkillInfo);
        return backupSkill;
    }

    public void SetActive(bool active) {
        backupSkillModel.SetActive(active);
        backupSkillView.SetActive(active);
    }

    public void OnBackupSkillSetPage() {
        backupPageView?.SetPage(backupSkillModel.page, backupSkillModel.lastPage);
    }

    public void OnBackupSkillPrevPage() {
        backupSkillModel.PrevPage();
        backupSkillView.SetNormalSkills(backupSkillModel.normalSkillInfos);
        if (backupSkillModel.isNormalSkillChosen) {
            backupSkillView.SelectNormalSkill(backupSkillModel.cursor[0]);
        }
        OnBackupSkillSetPage();
    }

    public void OnBackupSkillNextPage() {
        backupSkillModel.NextPage();
        backupSkillView.SetNormalSkills(backupSkillModel.normalSkillInfos);
        if (backupSkillModel.isNormalSkillChosen) {
            backupSkillView.SelectNormalSkill(backupSkillModel.cursor[0]);
        }
        OnBackupSkillSetPage();
    }

}

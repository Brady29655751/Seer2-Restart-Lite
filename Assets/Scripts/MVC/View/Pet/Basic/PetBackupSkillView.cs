using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetBackupSkillView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private Image background;
    [SerializeField] private IButton prevPageButton, nextPageButton;
    [SerializeField] private PetSkillBlockView superSkillBlockView;
    [SerializeField] private PetSkillBlockView[] normalSkillBlockViews = new PetSkillBlockView[4];

    public void SetPet(Pet pet) {
        if (pet == null)
            return;
        SetNormalSkills(pet.skills.GetLearnSkillInfos(pet.backupNormalSkill.Select(x => x.id).ToArray()));
        SetSuperSkill((pet.backupSuperSkill == null) ? null : pet.skills.GetLearnSkillInfos(pet.backupSuperSkill.id));
        SetActive(false);
    }

    public void SetActive(bool active) {
        background?.gameObject.SetActive(active);
    }

    public void SetNormalSkills(LearnSkillInfo[] normalSkillInfos) {
        Array.Resize(ref normalSkillInfos, 4);

        for (int i = 0; i < 4; i++) {
            normalSkillBlockViews[i].SetSkill(normalSkillInfos[i]);
        }
    }

    public void SetSuperSkill(LearnSkillInfo superSkillInfo) {
        superSkillBlockView.SetSkill(superSkillInfo);
    }

    public void SelectNormalSkill(int index) {
        if (!index.IsInRange(0, 4))
            return;

        for (int i = 0; i < 4; i++) {
            normalSkillBlockViews[i].SetChosen(i == index);
        }
    }

    public void SelectSuperSkill() {
        superSkillBlockView.SetChosen(true);
    }

    public void OnResetButtonClick() {    
        foreach (var block in normalSkillBlockViews) {
            block.SetChosen(false);
        }
        superSkillBlockView.SetChosen(false);
        background?.gameObject.SetActive(!background.gameObject.activeSelf);
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetSkillInfoPromptContent(Skill skill) {
        if (skill == null) {
            infoPrompt.SetActive(false);
            return;
        }

        infoPrompt.SetSkill(skill, false);
    }
}

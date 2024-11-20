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

    public void SetPet(LearnSkillInfo[] normalSkillInfos, LearnSkillInfo superSkillInfo) {
        // if (pet == null)
        //     return;

        SetNormalSkills(normalSkillInfos);
        SetSuperSkill(superSkillInfo);
        SetActive(false);
    }

    public void SetActive(bool active) {
        foreach (var block in normalSkillBlockViews)
            block.SetChosen(false);

        superSkillBlockView.SetChosen(false);
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
        for (int i = 0; i < 4; i++)
            normalSkillBlockViews[i].SetChosen(i == index);
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

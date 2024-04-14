using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSecretSkillView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private List<PetSkillBlockView> skillBlockViews;
    [SerializeField] private List<IText> learnConditionTexts;

    public void SetSecretSkillInfo(LearnSkillInfo[] secretSkillInfos) {
        for (int i = 0; i < Mathf.Min(secretSkillInfos.Length, 4); i++) {
            skillBlockViews[i]?.SetSkill(secretSkillInfos[i]);
            learnConditionTexts[i]?.SetText(secretSkillInfos[i].learnDescription);
        }
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetSkillInfoPromptContent(Skill skill) {
        if (skill == null) {
            infoPrompt.SetActive(false);
            return;
        }
        
        infoPrompt.SetSkill(skill);
    }
}

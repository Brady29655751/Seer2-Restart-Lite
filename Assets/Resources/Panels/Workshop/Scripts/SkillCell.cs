using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCell : UIModule
{
    public RectTransform rectTransform;
    [SerializeField] private IButton button;
    [SerializeField] private TextCell idCell, nameCell, typeCell, powerCell, angerCell, accuracyCell;
    [SerializeField] private Image elementImage;

    public Skill currentSkill { get; private set; }

    public void SetSkill(Skill skill) {
        currentSkill = skill;
        gameObject.SetActive(skill != null);
        if (skill == null)
            return;

        idCell.SetText(skill.id.ToString());
        nameCell.SetText(skill.name);
        typeCell.SetText(skill.type.ToString());
        typeCell.SetTextColor(ColorHelper.gold);
        powerCell.SetText(skill.power.ToString());
        angerCell.SetText(skill.anger.ToString());
        accuracyCell.SetText(skill.accuracy.ToString());
        accuracyCell.SetTextColor(ColorHelper.green);
        elementImage.SetElementSprite(skill.element);
    }

    public void SetInfoPrompt(InfoPrompt prompt) {
        infoPrompt = prompt;
    }

    public void SetCallback(Action<Skill> callback, string which = null) {
        Action skillCallback = () => {
            callback?.Invoke(currentSkill);
            SetInfoPromptActive(false);
        };

        switch (which) {
            default:
                button?.onPointerClickEvent?.SetListener(skillCallback.Invoke);
                return;
            case "id":
                idCell?.SetCallback(skillCallback);
                return;
        }
    }

    public void ShowSkillInfo() {
        infoPrompt?.SetSkill(currentSkill);
    }

}

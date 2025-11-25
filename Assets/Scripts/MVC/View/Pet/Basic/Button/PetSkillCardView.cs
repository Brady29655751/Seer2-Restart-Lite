using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSkillCardView : Module
{
    private Skill currentSkill;
    private bool isNull => currentSkill == null;

    [SerializeField] private IButton skillButton;
    [SerializeField] private Image artwork;
    [SerializeField] private Image angerTensDigit, angerUnitsDigit;
    [SerializeField] private IText nameText, effectText;

    public void SetSkill(Skill skill, BattleRule rule = BattleRule.Anger)
    {
        currentSkill = skill;
        skillButton.SetInteractable(!isNull, false);
        SetName();
        SetElement();
        SetAnger(rule);
        SetEffect();
        SetArtwork();
    }

    public void SetArtwork()
    {
        if (isNull)
        {
            artwork.sprite = SpriteSet.Empty;
            return;
        }

        artwork.SetSprite(ResourceManager.instance.GetSprite(currentSkill.id.ToString()));
    }

    private void SetName()
    {
        nameText.SetText(isNull ? string.Empty : currentSkill.name);
    }

    private void SetElement()
    {
        nameText.SetColor(isNull ? Color.white : PetElementSystem.GetElementColor(currentSkill.element));
    }

    private void SetAnger(BattleRule rule = BattleRule.Anger)
    {
        var anger = isNull ? -1 : rule switch
        {
            BattleRule.Anger => currentSkill.anger,
            BattleRule.PP => currentSkill.PP,
            _ => -1,
        };

        if (anger < 0)
        {
            angerTensDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/%"));
            angerUnitsDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/%"));
            return;
        }

        anger = Mathf.Min(anger, 0, 99);
        angerTensDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/{anger / 10}"));
        angerUnitsDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/{anger % 10}"));
    }

    private void SetEffect()
    {
        effectText.SetText(isNull ? string.Empty : currentSkill.description);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSkillBlockView : Module
{   
    private Skill currentSkill;
    private SecretType currentSecretType = SecretType.GreaterThanLevel;
    private bool isNull => (currentSkill == null);

    [SerializeField] private bool isSuperSkillBlock = false;
    [SerializeField] private IButton skillButton;
    [SerializeField] private Image innerBackground;
    [SerializeField] private Image elementImage;
    [SerializeField] private Text typeText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text powerText;
    [SerializeField] private Text angerText;

    public void SetSkill(LearnSkillInfo skillInfo, BattleRule rule = BattleRule.Anger) {
        currentSkill = skillInfo?.skill;
        currentSecretType = skillInfo?.secretType ?? SecretType.GreaterThanLevel;
        skillButton.SetInteractable(!isNull, false);
        SetName();
        SetElement();
        SetType();
        SetPowerAndAnger(rule);
        SetSecret(currentSecretType);
        SetChosen(false);
    }

    public void SetChosen(bool chosen) {
        skillButton.image.SetSkillBackgroundSprite(chosen, isSuperSkillBlock, currentSecretType);
    }

    private void SetName() {
        nameText.text = isNull ? string.Empty : currentSkill.name;
    }

    private void SetElement() {
        elementImage.gameObject.SetActive(!isNull);
        if (!isNull) {
            elementImage.SetElementSprite(currentSkill.element);
        }
    }

    private void SetType() {
        typeText.text = isNull ? string.Empty : currentSkill.type.ToString();
    }

    private void SetPowerAndAnger(BattleRule rule = BattleRule.Anger) {
        if (isNull) {
            powerText.text = string.Empty;
            angerText.text = string.Empty;
            return;
        } 
        powerText.text = $"威力: {currentSkill.power}";
        angerText.text = rule switch
        {
            BattleRule.Anger => $"怒气: {currentSkill.anger}",
            BattleRule.PP => $"次数: {currentSkill.maxPP}",
            _ => string.Empty,
        };
    }

    private void SetSecret(SecretType secretType) {
        if (isNull) {
            nameText.color = powerText.color = angerText.color = ColorHelper.normalSkill;
            innerBackground.gameObject.SetActive(false);
            return;
        }
        Color skillColor = secretType.GetSecretSkillColor();
        nameText.color = powerText.color = angerText.color = skillColor;
        innerBackground.gameObject.SetActive((secretType > SecretType.GreaterThanLevel) || (secretType < SecretType.Others));
    }


}

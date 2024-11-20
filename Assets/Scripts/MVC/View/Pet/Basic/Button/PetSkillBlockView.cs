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

    public void SetSkill(LearnSkillInfo skillInfo) {
        currentSkill = skillInfo?.skill;
        currentSecretType = skillInfo?.secretType ?? SecretType.GreaterThanLevel;
        skillButton.SetInteractable(!isNull, false);
        SetName();
        SetElement();
        SetType();
        SetPowerAndAnger();
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

    private void SetPowerAndAnger() {
        powerText.text = isNull ? string.Empty : ("威力: " + currentSkill.power.ToString());
        angerText.text = isNull ? string.Empty : ("怒气: " + currentSkill.anger.ToString());
    }

    private void SetSecret(SecretType secretType) {
        if (isNull) {
            nameText.color = powerText.color = angerText.color = ColorHelper.normalSkill;
            innerBackground.gameObject.SetActive(false);
            return;
        }
        bool isSecret = (secretType != SecretType.GreaterThanLevel);
        Color skillColor = isSecret ? ColorHelper.secretSkill : ColorHelper.normalSkill;
        nameText.color = powerText.color = angerText.color = skillColor;
        innerBackground.gameObject.SetActive(secretType > SecretType.GreaterThanLevel);
    }


}

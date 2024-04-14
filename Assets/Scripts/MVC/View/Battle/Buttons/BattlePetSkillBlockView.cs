using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetSkillBlockView : Module
{
    private Skill currentSkill;
    private bool isNull => (currentSkill == null);

    [SerializeField] private IButton skillButton;
    [SerializeField] private Image elementImage;
    [SerializeField] private Text typeText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text powerText;
    [SerializeField] private Text angerText;

    public void SetSkill(Skill skill) {
        currentSkill = skill;
        if (isNull) {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        SetName();
        SetElement();
        SetType();
        SetPowerAndAnger();
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

    public void SetInteractable(bool interactable) {
        skillButton.SetInteractable(interactable);
    }
}

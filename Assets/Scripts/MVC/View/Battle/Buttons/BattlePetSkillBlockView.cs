using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetSkillBlockView : Module
{
    private Skill currentSkill;
    private bool isNull => currentSkill == null;

    [SerializeField] private IButton skillButton;
    [SerializeField] private Image elementImage;
    [SerializeField] private Text typeText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text powerText;
    [SerializeField] private Text angerText;

    public void SetSkill(Skill skill, BattleRule rule = BattleRule.Anger) {
        currentSkill = skill;
        if (isNull) {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        SetName();
        SetElement();
        SetType();
        SetPowerAndAnger(rule);
    }

    private void SetName() {
        int length = isNull ? 0 : currentSkill.name.Length;
        nameText.text = isNull ? string.Empty : currentSkill.name;
        nameText.fontSize = 20 - Mathf.Max(length, 6);
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
        angerText.text = rule switch {
            BattleRule.Anger    => $"怒气: {currentSkill.anger}",
            BattleRule.PP       => $"PP: {currentSkill.pp}/{currentSkill.maxPP}",
            _                   => string.Empty,
        };
    }

    public void SetInteractable(bool interactable) {
        skillButton.SetInteractable(interactable);
    }
}

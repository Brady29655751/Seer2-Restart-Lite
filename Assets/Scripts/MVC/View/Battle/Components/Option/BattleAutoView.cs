using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleAutoView : BattleBaseView
{
    [SerializeField] private Hintbox skillOrderHintbox;
    [SerializeField] private IInputField skillOrderInputField;
    [SerializeField] private Toggle skillAutoSuperToggle;

    public string inputString => skillOrderInputField.inputString;
    public bool isAutoSuper => skillAutoSuperToggle.isOn;

    public void OnToggleAutoBattle(bool isOn) {
        battle.autoSkillCursor = 0;
        battle.autoSkillOrder.Clear();
        battle.isAutoSuperSkill = false;
        skillOrderHintbox?.SetActive(isOn);        
    }

    public void SetAutoSkillOrder() {
        var skillOrder = inputString.Replace("，", ",").ToIntList();
        if (ListHelper.IsNullOrEmpty(skillOrder)) {
            Hintbox.OpenHintboxWithContent("出招顺序填写不合规范", 16);
            return;
        }

        skillOrderHintbox?.SetActive(false);
        battle.autoSkillOrder = skillOrder;
        battle.isAutoSuperSkill = isAutoSuper;
        UI.CheckAutoSkill();
    }
}
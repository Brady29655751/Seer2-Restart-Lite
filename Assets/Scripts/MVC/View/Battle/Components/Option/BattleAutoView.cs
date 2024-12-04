using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleAutoView : BattleBaseView
{
    [SerializeField] private Hintbox skillOrderHintbox;
    [SerializeField] private IInputField skillOrderInputField;

    public string inputString => skillOrderInputField.inputString;

    public void OnToggleAutoBattle(bool isOn) {
        battle.autoSkillCursor = 0;
        battle.autoSkillOrder.Clear();
        skillOrderHintbox?.SetActive(isOn);        
    }

    public void SetAutoSkillOrder() {
        var skillOrder = inputString.ToIntList();
        if (ListHelper.IsNullOrEmpty(skillOrder)) {
            Hintbox.OpenHintboxWithContent("出招顺序填写不合规范", 16);
            return;
        }

        skillOrderHintbox?.SetActive(false);
        battle.autoSkillOrder = skillOrder;
        UI.CheckAutoSkill();
    }
}
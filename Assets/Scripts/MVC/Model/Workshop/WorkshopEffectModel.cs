using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopEffectModel : Module
{
    [SerializeField] private IInputField nameInputField;
    [SerializeField] private IDropdown timingDropdown, targetDropdown, conditionDropdown;
    [SerializeField] private IInputField condOptionInputField, priorityInputField;
    [SerializeField] private IDropdown abilityDropdown; //, abilityDetailDropdown;
    [SerializeField] private IInputField abilityOptionInputField;

    public Effect effect => GetEffect();
    public string effectName => nameInputField.inputString;
    public EffectTiming timing => GetEffectTiming();
    public int priority => int.Parse(priorityInputField.inputString);
    public EffectTarget target => (EffectTarget)(targetDropdown.value);
    public EffectCondition condition => (EffectCondition)(conditionDropdown.value);
    public string conditionOptions => string.IsNullOrEmpty(condOptionInputField.inputString) ? "none" : condOptionInputField.inputString;
    public EffectAbility ability => (EffectAbility)(abilityDropdown.value);
    // public int abilityDetail => abilityDetailDropdown.value;
    public string abilityOptions => abilityOptionInputField.inputString;

    public Effect GetEffect() {
        var condOptionDictList = new List<Dictionary<string, string>>();
        var abilityOptionDict = new Dictionary<string, string>();
        condOptionDictList.ParseMultipleOptions(conditionOptions);
        abilityOptionDict.ParseOptions(abilityOptions);
        abilityOptionDict.Set("name", effectName);

        return new Effect(timing, priority, target, condition, condOptionDictList, ability, abilityOptionDict);
    }

    public void SetReferredEffect(int index, Effect effect) {
        nameInputField.SetInputString(effect.abilityOptionDict.Get("name", "效果 " + (index + 1)));
        SetEffectTiming(effect);
        priorityInputField.SetInputString(effect.priority.ToString());
        targetDropdown.value = (int)effect.target;
        conditionDropdown.value = (int)effect.condition;
        condOptionInputField.SetInputString(effect.GetRawCondtionOptionString());
        abilityDropdown.value = ((int)effect.ability);
        abilityOptionInputField.SetInputString(effect.GetRawAbilityOptionString(new List<string>() { "name" }));
    }

    public EffectTiming GetEffectTiming() {
        const int prevStage = 5, midStage = 10, lastStage = 19;

        if (timingDropdown.value.IsWithin(1, prevStage))
            return (EffectTiming)(-timingDropdown.value - 1);

        if (timingDropdown.value.IsWithin(prevStage + 2, midStage))
            return (EffectTiming)(timingDropdown.value - prevStage);

        if (timingDropdown.value.IsWithin(midStage + 1, lastStage))
            return (EffectTiming)(timingDropdown.value - (midStage - 6));

        return timingDropdown.value switch {
            prevStage + 1  => EffectTiming.Resident,
            lastStage + 1  => EffectTiming.OnBattleEnd,
            _ => EffectTiming.None,
        };
    }

    private void SetEffectTiming(Effect effect) {
        for (timingDropdown.value = 0; timingDropdown.value < timingDropdown.optionCount; timingDropdown.value++) {
            if (effect.timing == GetEffectTiming())
                break;
        }
    }

    public bool VerifyDIYEffect(out string error) {
        error = string.Empty;

        if (!VerfiyName(out error))
            return false;

        if (!VerifyTimingAndPriority(out error))
            return false;

        if (!VerifyConditionOptions(out error))
            return false;

        if (!VerifyAbilityOptions(out error))
            return false;

        return true;
    }

    private bool VerfiyName(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(effectName)) {
            error = "名字不能为空！";
            return false;
        }

        if (effectName.Contains(',')) {
            error = "名字不能有半形逗号";
            return false;
        }
        return true;
    }

    private bool VerifyTimingAndPriority(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(priorityInputField.inputString)) {
            error = "结算顺序不能为空！";
            return false;
        }

        if (!int.TryParse(priorityInputField.inputString, out _)) {
            error = "结算顺序必须为整数";
            return false;
        }

        return true;
    }

    private bool VerifyConditionOptions(out string error) {
        error = string.Empty;

        if (conditionOptions.Contains(',')) {
            error = "自订条件细节不能有半形逗号";
            return false;
        }

        var dictList = new List<Dictionary<string, string>>();
        try {
            dictList.ParseMultipleOptions(conditionOptions);
        } catch (Exception) {
            error = "有重复或残缺的自订条件细节";
            return false;
        }

        return true;
    }

    private bool VerifyAbilityOptions(out string error) {
        error = string.Empty;

        if (abilityOptions.Contains(',')) {
            error = "自订能力细节不能有半形逗号";
            return false;
        }

        var dict = new Dictionary<string, string>();
        try {
            dict.ParseOptions(abilityOptions);
        } catch (Exception) {
            error = "有重复或残缺的自订能力细节";
            return false;
        }

        return true;
    }
}

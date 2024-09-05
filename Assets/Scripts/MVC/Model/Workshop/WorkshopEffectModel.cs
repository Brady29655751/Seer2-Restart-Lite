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
    public EffectAbility ability => (EffectAbility)(abilityDropdown.value + ((abilityDropdown.value < 3) ? 0 : 1));
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
        abilityDropdown.value = ((int)effect.ability) - ((((int)effect.ability) < 3) ? 0 : 1);
        abilityOptionInputField.SetInputString(effect.GetRawAbilityOptionString(new List<string>() { "name" }));
    }

    public EffectTiming GetEffectTiming() {
        if (timingDropdown.value.IsWithin(4, 7))
            return (EffectTiming)(timingDropdown.value - 1);

        if (timingDropdown.value.IsWithin(8, 15))
            return (EffectTiming)(timingDropdown.value);

        return timingDropdown.value switch {
            1   => EffectTiming.OnAddBuff,
            2   => EffectTiming.OnRemoveBuff,
            3   => EffectTiming.Resident,
            16  => EffectTiming.OnBattleEnd,
            _ => EffectTiming.None,
        };
    }

    private void SetEffectTiming(Effect effect) {
        for (timingDropdown.value = 0; timingDropdown.value < timingDropdown.optionCount; timingDropdown.value++) {
            if (effect.timing == GetEffectTiming())
                break;
        }
    }

    public void OnAbilityChanged() {
        List<string> abilityDetailOptions = ability switch {
            EffectAbility.Heal      => new List<string>() { "恢复体力", "修改体力", "秒杀" },
            EffectAbility.Rage      => new List<string>() { "恢复怒气", "不低于", "不高于" },
            EffectAbility.Powerup   => new List<string>() { "普通", "随机" },
            EffectAbility.AddStatus => new List<string>() { "增加能力值" },
            EffectAbility.BlockBuff => new List<string>() { "序号", "种类" },
            EffectAbility.AddBuff   => new List<string>() { "添加印记" },
            EffectAbility.RemoveBuff=> new List<string>() { "序号", "种类" },
            EffectAbility.CopyBuff  => new List<string>() { "序号", "种类" },
            EffectAbility.SetBuff   => new List<string>() { "回合", "数值" },
            EffectAbility.SetDamage => new List<string>() { "伤害乘算", "附带伤害", "固定伤害", "伤害不低于", "伤害不高于", "回合结束伤害" },
            EffectAbility.SetSkill  => new List<string>() { "修改技能数值", "改变使用技能" },
            EffectAbility.SetPet    => new List<string>() { "修改精灵数值" },
            EffectAbility.SetWeather=> new List<string>() { "修改天气" },
            _ => new List<string>() { "无" },
        };
        // abilityDetailDropdown.SetDropdownOptions(abilityDetailOptions);
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

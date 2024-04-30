using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect {

    public const int DATA_COL = 7;

    public object source = null;
    public object invokeUnit = null;

    public EffectTiming timing { get; private set; }
    public int priority { get; private set; }
    public EffectTarget target { get; private set; }
    public EffectCondition condition { get; private set; }
    public List<Dictionary<string, string>> condOptionDictList { get; private set; } = new List<Dictionary<string, string>>();
    public EffectAbility type => ability;
    public EffectAbility ability { get; private set; }
    public Dictionary<string, string> abilityOptionDict { get; private set; } = new Dictionary<string, string>();

    public Effect(string _timing, string _priority, string _target, string _condition, string _condition_option, string _ability, string _ability_option) {
        source = null;
        timing = _timing.ToEffectTiming();
        priority = int.Parse(_priority);
        target = _target.ToEffectTarget();
        condition = _condition.ToEffectCondition();
        condOptionDictList.ParseMultipleOptions(_condition_option);
        ability = _ability.ToEffectAbility();
        abilityOptionDict.ParseOptions(_ability_option);
    }

    public Effect(EffectTiming _timing, int _priority, EffectTarget _target, 
        EffectCondition _condition, List<Dictionary<string, string>> _condition_option,
        EffectAbility _ability, Dictionary<string, string> _ability_option) {
        source = null;
        timing = _timing;
        priority = _priority;
        target = _target;
        condition = _condition;
        if (_condition_option == null) {
            condOptionDictList.Add(new Dictionary<string, string>());
        } else {
            condOptionDictList = _condition_option;
        }
        ability = _ability;
        abilityOptionDict = _ability_option ?? new Dictionary<string, string>();
    }

    public Effect(Effect rhs) {
        timing = rhs.timing;
        priority = rhs.priority;
        target = rhs.target;
        condition = rhs.condition;
        ability = rhs.ability;
        condOptionDictList = rhs.condOptionDictList.Select(x => new Dictionary<string, string>(x)).ToList();
        abilityOptionDict = new Dictionary<string, string>(rhs.abilityOptionDict);
    }

    public static string[] GetRawEffectListStringArray(int id, List<Effect> effectList) {
        var result = Enumerable.Repeat(string.Empty, DATA_COL + 1).ToArray();
        var rawStringArrays = effectList.Select(x => x.GetRawEffectStringArray()).ToList();

        result[0] = id.ToString();

        for (int i = 0; i < DATA_COL; i++) {
            int copy = i;
            result[copy + 1] = rawStringArrays.Select(array => array[copy]).ConcatToString("\\");
        }
        return result;
    }

    public string[] GetRawEffectStringArray() {
        var rawConditionOptions = ((condOptionDictList.FirstOrDefault()?.Count ?? 0) == 0) ? "none" :
            condOptionDictList.Select(cond => cond.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&")).ConcatToString("|");
        var rawAbilityOptions = (abilityOptionDict.Count == 0) ? "none" : 
            abilityOptionDict.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");

        return new string[] { timing.ToRawString(), priority.ToString(), target.ToRawString(),
            condition.ToRawString(), rawConditionOptions, ability.ToRawString(), rawAbilityOptions };
    }

    public static Effect GetEscapeEffect() {
        return new Effect(EffectTiming.OnBeforeAttack, -1, EffectTarget.None, EffectCondition.None, null, EffectAbility.Escape, null);
    }

    public static Effect GetPetChangeEffect(int sourceIndex, int targetIndex, bool passive) {
        Dictionary<string, string> ability_option = new Dictionary<string, string>() { 
            { "source_index", sourceIndex.ToString() },
            { "target_index", targetIndex.ToString() },
            { "passive", passive.ToString() }
        };
        var phase = passive ? EffectTiming.OnPassivePetChange : EffectTiming.OnBeforeAttack;
        return new Effect(phase, -1, EffectTarget.CurrentPet, EffectCondition.None, null, EffectAbility.PetChange, ability_option);
    }

    public bool Condition(object invokeUnit, BattleState state, bool checkPhase = true, bool checkTurn = true) {
        bool isCorrectPhase = ((state == null) && (timing == EffectTiming.Resident)) || 
            ((state != null) && (state.phase == timing));

        if (checkPhase && !isCorrectPhase)
            return false;

        this.invokeUnit = invokeUnit;

        if (checkTurn && !condOptionDictList.Select(x => this.IsCorrectTurn(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.IsCorrectWeather(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.IsAttackAndHit(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.RandomNumber(state, x)).Any(x => x))
            return false;
        
        Func<Dictionary<string, string>, bool> ConditionFunc = condition switch {
            EffectCondition.CurrentUnit => ((x) => this.UnitCondition(state, x)),
            EffectCondition.CurrentPet => ((x) => this.PetCondition(state, x)),
            EffectCondition.CurrentStatus => ((x) => this.StatusCondition(state, x)),
            EffectCondition.CurrentBuff => ((x) => this.BuffCondition(state, x)),
            EffectCondition.CurrentSkill => ((x) => this.SkillCondition(state, x)),
            EffectCondition.LastSkill => ((x) => this.SkillCondition(state, x)),
            _ => ((x) => true)
        };

        return condOptionDictList.Select(x => ConditionFunc.Invoke(x)).Any(x => x);
    }

    public bool Apply(object invokeUnit, BattleState state = null) {
        this.invokeUnit = invokeUnit;
        int repeat = int.Parse(abilityOptionDict.Get("repeat", "1"));
        bool result = true;

        for (int i = 0; i < repeat; i++) {
            result &= (type switch {
                EffectAbility.Win => this.Win(state),
                EffectAbility.Escape => this.Escape(state),
                EffectAbility.Capture => this.Capture(state),
                EffectAbility.PetChange => this.PetChange(state),
                EffectAbility.Heal => this.Heal(state),
                EffectAbility.Rage => this.Rage(state),
                EffectAbility.Powerup => this.Powerup(state),
                EffectAbility.AddStatus => this.AddStatus(state),
                EffectAbility.BlockBuff => this.BlockBuff(state),
                EffectAbility.AddBuff => this.AddBuff(state),
                EffectAbility.RemoveBuff => this.RemoveBuff(state),
                EffectAbility.CopyBuff => this.CopyBuff(state),
                EffectAbility.SetBuff => this.SetBuff(state),
                EffectAbility.SetDamage => this.SetDamage(state),
                EffectAbility.SetSkill => this.SetSkill(state),
                EffectAbility.SetPet => this.SetPet(state),
                EffectAbility.SetWeather => this.SetWeather(state),
                _ => true
            });
        }
        return result;
    }

    public bool CheckAndApply(object invokeUnit, BattleState state = null, bool checkPhase = true, bool checkTurn = true) {
        if (!Condition(invokeUnit, state, checkPhase, checkTurn))
            return false;
        
        return Apply(invokeUnit, state);
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect {

    public const int DATA_COL = 7;

    public object source = null;
    public object invokeUnit = null;

    public int id = 0;
    public EffectTiming timing { get; private set; }
    public int priority { get; private set; }
    public EffectTarget target { get; private set; }
    public string[] targetType => abilityOptionDict.Get("target_type", "none").Split('_');
    public string targetFilter => abilityOptionDict.Get("target_filter", "none");
    public EffectCondition condition { get; private set; }
    public List<Dictionary<string, string>> condOptionDictList { get; private set; } = new List<Dictionary<string, string>>();
    public EffectAbility type => ability;
    public EffectAbility ability { get; private set; }
    public Dictionary<string, string> abilityOptionDict { get; private set; } = new Dictionary<string, string>();

    public bool isSelect => IsSelect();

    public Effect(){}
    public Effect(string _timing, string _priority, string _target, string _condition, string _condition_option, string _ability, string _ability_option)
    {
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
        id = rhs.id;
        timing = rhs.timing;
        priority = rhs.priority;
        target = rhs.target;
        condition = rhs.condition;
        ability = rhs.ability;
        condOptionDictList = rhs.condOptionDictList.Select(x => new Dictionary<string, string>(x)).ToList();
        abilityOptionDict = new Dictionary<string, string>(rhs.abilityOptionDict);
    }

    public static string[] GetRawEffectListStringArray(int id, List<Effect> effectList) {
        if (effectList.Count == 0)
            return (new string[] { id.ToString() }).Concat(Effect.GetDefaultEffect().GetRawEffectStringArray()).ToArray();

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
        return new string[] { timing.ToRawString(), priority.ToString(), target.ToRawString(),
            condition.ToRawString(), GetRawCondtionOptionString(), ability.ToRawString(), GetRawAbilityOptionString() };
    }

    public string GetRawCondtionOptionString() {
        return ((condOptionDictList.FirstOrDefault()?.Count ?? 0) == 0) ? "none" :
            condOptionDictList.Select(cond => cond.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&")).ConcatToString("|");
    }

    public string GetRawAbilityOptionString(List<string> excludeKeys = null) {
        if (abilityOptionDict.Count == 0)
            return "none";

        var dict = new Dictionary<string, string>(abilityOptionDict);
        if (!ListHelper.IsNullOrEmpty(excludeKeys))
            excludeKeys.ForEach(x => dict.Remove(x));

        return dict.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");
    }

    public static Effect GetEffect(int id)
    {
        var e = Database.instance.GetEffect(id);
        return (e == null) ? null : new Effect(e);
    }

    /// <summary>
    /// Format the effects and return
    /// </summary>
    /// <param name="effectsBeforeFormat"></param>
    /// <returns>Effects after formatting {1}, {2}, ... in options</returns>
    public static List<Effect> SetEffects(List<Effect> effectsBeforeFormat, EffectAbility formatAbility = EffectAbility.SetSkill)
    {
        var effects = new List<Effect>();

        foreach (var e in effectsBeforeFormat)
        {
            if ((e.timing == EffectTiming.Resident) && (e.ability == formatAbility) && (e.abilityOptionDict.Get("type") == "effect"))
            {
                var value = e.abilityOptionDict.Get("value");
                if (value == null)
                    continue;

                var split = value.TrimParenthesesLoop('(', ')');
                for (int i = 0; i < split.Count; i++)
                {
                    var startIndex = split[i].IndexOf('{');
                    var id = split[i].Substring(0, (startIndex == -1) ? split[i].Length : startIndex);
                    var options = split[i].TrimParenthesesLoop('{', '}');
                    var r = Effect.GetEffect(int.Parse(id));

                    r.Format(options);
                    effects.Add(r);
                }
            }
            else
            {
                effects.Add(e);   
            }
        }

        return effects;
    }

    public static Effect GetDefaultEffect() {
        return new Effect(EffectTiming.None, -1, EffectTarget.None, EffectCondition.None, null, EffectAbility.None, null);
    }

    public static Effect GetEscapeEffect() => Effect.GetEffect(-4);
    public static Effect GetPetChangeEffect(int sourceIndex, int targetIndex, bool passive) {
        Dictionary<string, string> ability_option = new Dictionary<string, string>() { 
            { "source_index", sourceIndex.ToString() },
            { "target_index", targetIndex.ToString() },
            { "passive", passive.ToString() }
        };
        var phase = passive ? EffectTiming.OnPassivePetChange : EffectTiming.OnBeforeAttack;
        return new Effect(phase, -1, EffectTarget.CurrentPet, EffectCondition.None, null, EffectAbility.PetChange, ability_option);
    }

    public void SetTiming(EffectTiming timing) {
        this.timing = timing;
    }

    /// <summary>
    /// Format the {0}, {1}, {2} ... text in effect options with given options.
    /// </summary>
    public void Format(List<string> options)
    {
        if (ListHelper.IsNullOrEmpty(options))
            return;

        for (int i = 0; i < options.Count; i++)
        {
            condOptionDictList = condOptionDictList.Select(x => x.ToDictionary(entry => entry.Key.Replace("{" + i + "}", options[i]), entry => entry.Value.Replace("{" + i + "}", options[i]))).ToList();
            abilityOptionDict = abilityOptionDict.ToDictionary(entry => entry.Key.Replace("{" + i + "}", options[i]), entry => entry.Value.Replace("{" + i + "}", options[i]));
        }
    }

    public bool IsSelect() {
        return targetType.Contains("index");
    }

    public bool Condition(object invokeUnit, BattleState state, bool checkPhase = true, bool checkTurn = true) {
        bool isCorrectPhase = ((state == null) && (timing == EffectTiming.Resident)) || 
            ((state != null) && ((timing == state.phase) || ((state.phase > EffectTiming.Resident) && (timing == EffectTiming.All))));

        if (checkPhase && !isCorrectPhase)
            return false;

        this.invokeUnit = invokeUnit;

        /*
        if (checkTurn && !condOptionDictList.Select(x => this.IsCorrectTurn(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.IsCorrectWeather(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.IsAttackAndHit(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.RandomNumber(state, x)).Any(x => x))
            return false;
        */

        Func<Dictionary<string, string>, bool> ConditionFunc = condition switch {
            EffectCondition.CurrentUnit => ((x) => this.UnitCondition(state, x)),
            EffectCondition.CurrentPet => ((x) => this.PetCondition(state, x)),
            EffectCondition.CurrentToken => ((x) => this.PetCondition(state, x)),
            EffectCondition.CurrentStatus => ((x) => this.StatusCondition(state, x)),
            EffectCondition.CurrentBuff => ((x) => this.BuffCondition(state, x)),
            EffectCondition.CurrentSkill => ((x) => this.SkillCondition(state, x)),
            EffectCondition.LastSkill => ((x) => this.SkillCondition(state, x)),
            EffectCondition.Poker => ((x) => this.PokerCondition(state, x)),
            _ => ((x) => true)
        };

        var result = condOptionDictList.Select(x => {
            /*
            var correctTurn = this.IsCorrectTurn(state, x);
            var correctWeather = this.IsCorrectWeather(state, x);
            var hit = this.IsAttackAndHit(state, x);
            var rng = this.RandomNumber(state, x);
            var cond = ConditionFunc.Invoke(x);

            if ((source.GetType() == typeof(Buff)) && ((Buff)source).id == -9999)
                Debug.Log(state.phase + " " + timing + " " + correctTurn + " " + hit + " " + rng + " " + cond);

            return ((!checkTurn) || correctTurn) && correctWeather && hit && rng && cond;
            */
            return ((!checkTurn) || this.IsCorrectTurn(state, x)) && 
                this.IsCorrectWeather(state, x) && this.IsAttackAndHit(state, x) && 
                this.RandomNumber(state, x) && ConditionFunc.Invoke(x);
            
        }).Any(x => x);

        return result;
    }

    public bool Apply(object invokeUnit, BattleState state = null) {
        this.invokeUnit = invokeUnit;
        var repeatExpr = abilityOptionDict.Get("repeat", "1");
        int repeat = 1;

        if (state == null)
            repeat = int.Parse(repeatExpr);
        else {
            Unit lhsUnit = (Unit)invokeUnit;
            Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
            repeat = (int)Parser.ParseEffectOperation(repeatExpr, this, lhsUnit, rhsUnit);
        }

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
                EffectAbility.BlockBuff => this.BlockOrCopyBuff(state),
                EffectAbility.AddBuff => this.AddBuff(state),
                EffectAbility.RemoveBuff => this.RemoveBuff(state),
                EffectAbility.CopyBuff => this.CopyBuff(state),
                EffectAbility.SetBuff => this.SetBuff(state),
                EffectAbility.SetDamage => this.SetDamage(state),
                EffectAbility.SetSkill => this.SetSkill(state),
                EffectAbility.SetPet => this.SetPet(state),
                EffectAbility.SetWeather => this.SetWeather(state),
                EffectAbility.SetPlayer => this.SetPlayer(state),
                EffectAbility.Poker => this.Poker(state),
                EffectAbility.SetPetBag => this.SetPetBag(state),
                EffectAbility.Card => this.Card(state),
                _ => true
            });
        }

        // Post Process
        var postSkills = abilityOptionDict.Get("on_" + (result ? "success" : "fail")).ToIntList('/');
        var postEffects = postSkills?.Select(skillId => {
            // Get Post Effects
            var effects = Skill.GetSkill(skillId, false)?.effects?.Select(x => new Effect(x)).ToList();
            if (effects == null)
                return effects;

            // Fix timing
            foreach (var e in effects) {
                if (e.timing == EffectTiming.None)
                    e.SetTiming(timing);
            }
            
            return effects;
        }).ToList();

        var effectHandler = new EffectHandler();

        postEffects?.ForEach(e => effectHandler.AddEffects(invokeUnit, e));
        effectHandler.CheckAndApply(state);

        return result;
    }

    public bool CheckAndApply(object invokeUnit, BattleState state = null, bool checkPhase = true, bool checkTurn = true) {
        if (!Condition(invokeUnit, state, checkPhase, checkTurn))
            return false;
        
        return Apply(invokeUnit, state);
    }
}
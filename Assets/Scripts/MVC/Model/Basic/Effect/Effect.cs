using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect {

    public const int DATA_COL = 7;

    public object source = null;
    public BattlePet sourcePet = null;
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

    public static List<Effect> Parse(string value)
    {
        if (value == null)
            return null;

        var effects = new List<Effect>();
        var split = value.TrimParenthesesLoop('(', ')');

        if (split == null)
            return null;

        for (int i = 0; i < split.Count; i++)
        {
            var startIndex = split[i].IndexOf('{');
            var id = split[i].Substring(0, (startIndex == -1) ? split[i].Length : startIndex);
            var options = split[i].TrimParenthesesLoop('{', '}');
            var r = Effect.GetEffect(int.Parse(id));
            r.Format(options);
            effects.Add(r);
        }

        return effects;
    }

    public static bool TryParse(string value, out List<Effect> effects)
    {
        effects = Parse(value);
        return value.TryTrimParentheses(out _, '(', ')');
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
                if (string.IsNullOrEmpty(value))
                    continue;
                
                effects.AddRange(Effect.Parse(value));
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

        if (checkTurn && !condOptionDictList.Exists(x => this.IsCorrectTurn(state, x)))
            return false;

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
            
        var result = condOptionDictList.Exists(x => {
            /*
            var correctWeather = this.IsCorrectWeather(state, x);
            var hit = this.IsAttackAndHit(state, x);
            var rng = this.RandomNumber(state, x);
            var cond = ConditionFunc.Invoke(x);

            if ((source.GetType() == typeof(Buff)) && ((Buff)source).id == -9999)
                Debug.Log(state.phase + " " + timing + " " + correctTurn + " " + hit + " " + rng + " " + cond);

            return ((!checkTurn) || correctTurn) && correctWeather && hit && rng && cond;
            */
            return this.IsCorrectWeather(state, x) && this.IsAttackAndHit(state, x) && 
                this.RandomNumber(state, x) && ConditionFunc.Invoke(x);
            
        });

        // 未触发则改变本条效果
        if (!result)
        {
            var postExpr = condOptionDictList.Select(x => x?.Get("on_fail")).FirstOrDefault(x => x != null); 
            if ((!TryGetPostProcessEffects(postExpr, state, out var postEffects)) || ListHelper.IsNullOrEmpty(postEffects))
                return result;            
        
            var effect = postEffects.FirstOrDefault();
            if (effect == null)
                return result;

            target = effect.target;
            condOptionDictList.ForEach(x => x?.Remove("on_fail"));
            ability = effect.ability;
            abilityOptionDict = effect.abilityOptionDict;
            result = true;
        }

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
                _ => this.DefaultAbility(state),
            });
        }

        // Post Process
        var postExpr = abilityOptionDict.Get("on_" + (result ? "success" : "fail"));
        if (!TryGetPostProcessEffects(postExpr, state, out var postEffects))
            return result;

        var effectHandler = new EffectHandler();
        effectHandler.AddEffects(invokeUnit, postEffects);
        effectHandler.CheckAndApply(state);

        return result;
    }

    public bool CheckAndApply(object invokeUnit, BattleState state = null, bool checkPhase = true, bool checkTurn = true, BattlePet sourcePet = null) {
        this.sourcePet = sourcePet;
        if (!Condition(invokeUnit, state, checkPhase, checkTurn))
            return false;   
        
        return Apply(invokeUnit, state);
    }

    private bool TryGetPostProcessEffects(string postExpr, BattleState state, out List<Effect> postEffects)
    {
        postEffects = new List<Effect>();
        if (string.IsNullOrEmpty(postExpr))
            return false;

        if (!Effect.TryParse(postExpr, out postEffects))
        {
            var effectIdList = (state == null) ? postExpr.ToIntList('/') : postExpr.Split('/').Select(x => 
                (int)Parser.ParseEffectOperation(x, this, state.GetUnitById(((Unit)invokeUnit).id), state.GetRhsUnitById(((Unit)invokeUnit).id))).ToList();

            postEffects = effectIdList?.Select(skillId => 
                Skill.GetSkill(skillId, false)?.effects?.Select(x => new Effect(x)).ToList()
            ).SelectMany(x => x).ToList();
        } 

        // Fix timing
        foreach (var e in postEffects) {
            if (e.timing == EffectTiming.None)
                e.SetTiming(timing);
        }

        return true;
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Parser {
    public static void ParseOptions(this Dictionary<string, string> dict, string _data) {
        _data = _data.Trim();
        if (_data == "none")
            return;

        var _options = _data.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < _options.Length; i++) {
            var result = _options[i].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
            if (result.Length != 2) {
                throw new Exception("Option Parsing Failure");
            }
            dict.Add(result[0], result[1]);
        }
    }

    public static void ParseMultipleOptions(this List<Dictionary<string, string>> dictList, string _data) {
        _data = _data.Trim();
        if (_data == "none") {
            dictList.Add(new Dictionary<string, string>());
            return;
        }
        string[] _options_list = _data.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < _options_list.Length; i++) {
            Dictionary<string, string> opt_dict = new Dictionary<string, string>();
            opt_dict.ParseOptions(_options_list[i]);
            dictList.Add(opt_dict);
        }
    }

    public static bool IsCondition(string cond) {
        return Operator.condDict.ContainsKey(cond);
    }

    public static bool IsOperator(string op) {
        return Operator.opDict.ContainsKey(op);
    }

    public static bool IsFraction(string frac) {
        string[] num = frac.Split('/');
        if (num.Length != 2)
            return false;
        int _;
        return (int.TryParse(num[0], out _) && (int.TryParse(num[1], out _)));
    }

    public static KeyValuePair<DataType, object> ParseDataType(string data) {
        int resint; 
        float resfloat;

        if (string.IsNullOrEmpty(data))
            return new KeyValuePair<DataType, object>(DataType.Null, data);

        if (IsCondition(data))
            return new KeyValuePair<DataType, object>(DataType.Condition, Operator.condDict.Get(data));

        if (IsOperator(data))
            return new KeyValuePair<DataType, object>(DataType.Operator, Operator.opDict.Get(data));

        if (IsFraction(data))
            return new KeyValuePair<DataType, object>(DataType.Fraction, ParseFraction(data));

        if (int.TryParse(data, out resint))
            return new KeyValuePair<DataType, object>(DataType.Int, resint);

        if (float.TryParse(data, out resfloat))
            return new KeyValuePair<DataType, object>(DataType.Float, resfloat);

        return new KeyValuePair<DataType, object>(DataType.Text, data);
    }

    public static KeyValuePair<int, int> ParseFraction(string frac) {
        string[] num = frac.Split('/');
        int a = int.Parse(num[0]);
        int b = int.Parse(num[1]);
        return new KeyValuePair<int, int>(a, b);
    }

    public static float ParseEffectOperation(string expr, Effect effect, Unit lhsUnit, Unit rhsUnit, object otherSource = null) {
        bool negativeFirst = expr.StartsWith("-");
        float sign = negativeFirst ? -1 : 1;

        if (negativeFirst)
            expr = expr.Substring(1);

        string[] id = expr.Split(Operator.opDict.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        float value = Identifier.GetIdentifier(id[0], effect, lhsUnit, rhsUnit, otherSource);
        if (id.Length == 1)
            return sign * value;

        for (int i = 1, opStartIdx = id[0].Length, opEndIdx = 0; i < id.Length; i++) {
            opEndIdx = expr.IndexOf(id[i], opStartIdx);
            string op = expr.Substring(opStartIdx, opEndIdx - opStartIdx);
            value = Operator.Operate(op, value, Identifier.GetIdentifier(id[i], effect, lhsUnit, rhsUnit, otherSource));
            opStartIdx = opEndIdx + id[i].Length;
        }
        return sign * value;
    }

    public static float ParseOperation(string expr) {
        bool negativeFirst = expr.StartsWith("-");
        float sign = negativeFirst ? -1 : 1;
        if (negativeFirst)
            expr = expr.Substring(1);

        string[] id = expr.Split(Operator.opDict.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        float value = Identifier.GetIdentifier(id[0]);
        if (id.Length == 1)
            return sign * value;   

        for (int i = 1, opStartIdx = id[0].Length, opEndIdx = 0; i < id.Length; i++) {
            opEndIdx = expr.IndexOf(id[i], opStartIdx);
            string op = expr.Substring(opStartIdx, opEndIdx - opStartIdx);
            value = Operator.Operate(op, value, Identifier.GetIdentifier(id[i]));
            opStartIdx = opEndIdx + id[i].Length;
        }
        return sign * value;
    }

    public static List<BattlePet> GetBattlePetTargetList(BattleState state, Effect effect, Unit lhsUnit, Unit rhsUnit) {
        return effect.target switch {
            EffectTarget.CurrentPetBag => GetBattlePetTargetListFromPetBag(effect, lhsUnit, rhsUnit),
            _ => GetBattlePetTargetListFromDefault(state, effect, lhsUnit, rhsUnit),
        };
    }

    public static List<BattlePet> GetBattlePetTargetListFromDefault(BattleState state, Effect effect, Unit lhsUnit, Unit rhsUnit) {
        var phase = state.phase;
        state.phase = EffectTiming.OnSelectTarget;

        var handler = state.GetEffectHandler(lhsUnit, false);
        var condition = handler.Condition(state);
        var lastEffect = handler.GetEffects((x, i) => condition[i] && (effect.ability == effect.ability)).LastOrDefault();

        state.phase = phase;

        return (lastEffect == null) ? new List<BattlePet>() { lhsUnit.pet } :
            Parser.GetBattlePetTargetListFromPetBag(lastEffect, lhsUnit, rhsUnit);
    }

    public static List<BattlePet> GetBattlePetTargetListFromPetBag(Effect effect, Unit lhsUnit, Unit rhsUnit) {
        var targetList = new List<BattlePet>();
        
        var targetType = effect.targetType;
        var targetFilter = Parser.ParseBattlePetTargetFilter(effect, lhsUnit, rhsUnit);
        var targetNum = (int)Parser.ParseEffectOperation(effect.abilityOptionDict.Get("target_num", "-1"), effect, lhsUnit, rhsUnit);
        var targetIndex = (int)Parser.ParseEffectOperation(effect.abilityOptionDict.Get("target_index", "-1"), effect, lhsUnit, rhsUnit);

        var petBag = lhsUnit.petSystem.petBag;
        targetList = petBag.Where(x => x != null).Where(targetFilter).ToList();

        if (targetType.Contains("other"))
            targetList.Remove(lhsUnit.pet);

        if (targetType.Contains("random"))
            targetList = targetList.Random(targetNum, false);
        else if (targetType.Contains("index") && (targetIndex.IsInRange(0, petBag.Length)))
            targetList = new List<BattlePet>(){ petBag[targetIndex] };
        else if (targetNum >= 0)
            targetList = targetList.Take(targetNum).ToList();

        return targetList;
    }

    public static Func<BattlePet, bool> ParseBattlePetTargetFilter(Effect effect, Unit lhsUnit, Unit rhsUnit) {
        var targetFilter = effect.targetFilter;
        bool Filter(BattlePet pet) {
            if (targetFilter == null)
                return true;

            foreach (var filter in targetFilter) {
                var op = "=";
                var options = filter.Split(':');
                if (options.Length != 2) {
                    var split = Operator.SplitCondition(filter, out op);
                    options = new string[] { split.Key, split.Value };  
                }
                var condition = Operator.Condition(op, 
                    Identifier.GetPetIdentifier(options[0], lhsUnit.petSystem, pet),
                    Identifier.GetPetIdentifier(options[1], lhsUnit.petSystem, pet)
                );
                if (!condition)
                    return false;
            }
            return true;
        }
        return Filter;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EffectConditionHandler
{
    public static Player player => Player.instance;
    public static Battle battle => player.currentBattle;

    public static bool IsCorrectTurn(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        if (state == null)
            return true;

        string turn = condOptions.Get("turn_num", "all");
        bool isTurnNumCorrect = (turn == "all") || ((turn == "odd") && (state.turn % 2 == 1)) || ((turn == "even") && (state.turn % 2 == 0));

        if (state.whosTurn == 0)
            return isTurnNumCorrect;

        string who = condOptions.Get("whos_turn", "me");
        Unit invokeUnit = (Unit)effect.invokeUnit;

        bool isWhosTurnCorrect = (who == "all") || (invokeUnit.id == ((who == "me") ? state.atkUnit.id : state.defUnit.id));
        return isWhosTurnCorrect && isTurnNumCorrect;
    }

    public static bool IsCorrectWeather(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        if (state == null)
            return true;

        string weatherOp = condOptions.Get("weather_op", "=");
        string weather = condOptions.Get("weather", "all");
        if (weather == "all")
            return true;

        if (!int.TryParse(weather, out int value))
            return false;

        return Operator.Condition(weatherOp, state.weather, value);
    }

    //! Note that whos_attack must be applied when phase is OnAttack/OnAfterAttack
    public static bool IsAttackAndHit(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        if (state == null)
            return true;

        string who = condOptions.Get("whos_attack", "me");
        if (who == "all")
            return true;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);

        bool isAttackPhase = (state.phase == EffectTiming.OnAttack) || (state.phase == EffectTiming.OnAfterAttack);
        bool isPetChangePhase = (state.phase == EffectTiming.OnBeforePetChange) || (state.phase == EffectTiming.OnAfterPetChange);
        if (isPetChangePhase)
            return (lhsUnit.skill?.type ?? SkillType.空过) == SkillType.換场;

        if (!isAttackPhase)
            return true;

        string hit = condOptions.Get("is_hit", "true");
        string move = condOptions.Get("is_move", "true");
        bool isHit = true, isMove = true;
        
        if ((hit != "all") && !bool.TryParse(hit, out isHit))
            return false;
        
        if ((move != "all") && !bool.TryParse(move, out isMove))
            return false;

        return ((hit == "all") || (lhsUnit.skillSystem.isHit == isHit)) && ((move == "all") || (lhsUnit.pet.isMovable == isMove));
    }

    public static bool RandomNumber(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        string type = condOptions.Get("random_type", "rng");
        var isOpExist = condOptions.TryGet("random_op", out var op, "LTE");
        var isValueExist = condOptions.TryGet("random_cmp", out var cmpValue, "100");

        // No random condition if no specify.
        if ((!isOpExist) && (!isValueExist))
            return true;

        float random = Player.instance.random;
        float value;

        Unit lhsUnit = null, rhsUnit = null;

        if (state == null) {
            if (type == "rng")
                Player.instance.random = Random.Range(0, 100);
        } else {
            var invokeUnitId = ((Unit)effect.invokeUnit).id;
            lhsUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
            rhsUnit = state.GetRhsUnitById(lhsUnit.id);
            
            random = (type == "rng") ? Random.Range(0, 100) : lhsUnit.random;
            random *= (lhsUnit.pet.buffController.GetBuff(75)?.value ?? 100f) / 100f;
            random += lhsUnit.pet.buffController.GetBuff(67)?.value ?? 0;
            random *= (lhsUnit.pet.buffController.GetBuff(76)?.value ?? 100f) / 100f;
            random = lhsUnit.pet.buffController.GetBuff(68)?.value ?? random;
        }

        if (op == "IN") {
            int middleIndex = cmpValue.IndexOf('~');
            if (middleIndex == -1)
                return false;

            string startExpr = cmpValue.Substring(0, middleIndex);
            string endExpr = cmpValue.Substring(middleIndex + 1);
            int startRange = int.Parse(startExpr);
            int endRange = int.Parse(endExpr);
            return random.IsWithin(startRange, endRange);
        } 
            
        value = (state == null) ? float.Parse(cmpValue) : Parser.ParseEffectOperation(cmpValue, effect, lhsUnit, rhsUnit);
        return Operator.Condition(op, random, value);
    }

    public static bool UnitCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        string type = condOptions.Get("type", "none");
        string[] typeList = type.Split('/');

        var invokeUnitId = ((Unit)effect.invokeUnit).id;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get("type[" + i + "].op", "=");
            string cmpValue = condOptions.Get("type[" + i + "].cmp", "0");

            if (!Operator.Condition(op,
                    Parser.ParseEffectOperation(typeList[i], effect, lhsUnit, rhsUnit),
                    Parser.ParseEffectOperation(cmpValue, effect, lhsUnit, rhsUnit)))
                return false;
        }
        return true;
    }

    public static bool PetCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        string type = condOptions.Get("type", "none");
        string[] typeList = type.Split('/');

        if (type == "none")
            return true;

        if (state == null) {
            Pet pet = ((Pet)effect.invokeUnit);
            for (int i = 0; i < typeList.Length; i++) {
                string op = condOptions.Get(typeList[i] + "_op", "=");
                string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
                float value = pet.TryGetPetIdentifier(cmpValue, out value) ? 
                    value : Identifier.GetNumIdentifier(cmpValue);

                if (!Operator.Condition(op, pet.GetPetIdentifier(typeList[i]), value))
                    return false;
            }
            return true;
        }

        var invokeUnitId = ((Unit)effect.invokeUnit).id;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        BattlePet battlePet = lhsUnit.pet;

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get(typeList[i] + "_op", "=");
            string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
            float value = Parser.ParseEffectOperation(cmpValue, effect, lhsUnit, rhsUnit);
            if (!Operator.Condition(op, Identifier.GetPetIdentifier(typeList[i], lhsUnit.petSystem), value))
                return false;
        }
        return true;
    }

    public static bool StatusCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        string statusType = condOptions.Get("status_type", "hp");
        string op = condOptions.Get("op", "=");
        string cmpValue = condOptions.Get("cmp_value", "1/1");
        var data = Parser.ParseDataType(cmpValue);
        
        var invokeUnit = effect.invokeUnit;
        float currentValue = 0, maxValue = 0;

        if (state == null) {
            currentValue = ((Pet)invokeUnit).currentStatus[statusType];
            maxValue = ((Pet)invokeUnit).normalStatus[statusType];
        } else {
            var invokeUnitId = ((Unit)invokeUnit).id;
            var statusUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
            
            if (statusType == "hp") {
                currentValue = statusUnit.pet.hp;
                maxValue = statusUnit.pet.maxHp;
            } else if (statusType == "anger") {
                currentValue = statusUnit.pet.anger;
                maxValue = statusUnit.pet.maxAnger;
            } else {
                currentValue = statusUnit.pet.battleStatus[statusType];
                maxValue = statusUnit.pet.initStatus[statusType];
            }
        }
        return Operator.Condition(op, currentValue, maxValue, data);
    }

    public static bool BuffCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string key = condOptions.Get("key", string.Empty);
        string who = condOptions.Get("who", "me");
        string id = condOptions.Get("id", "0");
        string own = condOptions.Get("own", "true");
        string type = condOptions.Get("type", "none");
        
        string[] typeList = type.Split('/');

        if (!bool.TryParse(own, out bool ownBuff))
            return false;

        var invokeUnitId = ((Unit)effect.invokeUnit).id;
        Unit buffUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = state.GetRhsUnitById(buffUnit.id);
        var pet = buffUnit.pet;

        Buff buff = null;
        BuffType buffType;
        if (!id.TryTrimParentheses(out _, '(', ')'))
        {
            if (!int.TryParse(id, out int buffId))
            {
                buffType = id.ToBuffType();
                if (buffType != BuffType.None)
                    return ownBuff != ListHelper.IsNullOrEmpty(pet.buffController.GetRangeBuff(x => x.IsType(buffType)));
            }
            else
            {
                if (string.IsNullOrEmpty(key) && (buffId == 0))
                    return true;

                id = (string.IsNullOrEmpty(key) || (key == "unit")) ? $"(id:{buffId})" : null;
            }   
        }

        var buffFilter = Parser.ParseConditionFilter<Buff>(id, (expr, buff) => buff.TryGetBuffIdentifier(expr, out var num) ? num : Identifier.GetNumIdentifier(expr));

        if (key == "unit")
            buff = buffUnit.unitBuffs.Find(x => buffFilter(x));
        else if (!string.IsNullOrEmpty(key))
            buff = state.stateBuffs.FindAll(x => (x.Key == key) && buffFilter(x.Value)).FirstOrDefault().Value;
        else
            buff = pet.buffController.GetRangeBuff(x => buffFilter(x)).FirstOrDefault();

        bool isOwnCorrect = (ownBuff == (buff != null));

        if ((!ownBuff) || (!isOwnCorrect) || (type == "none"))
            return isOwnCorrect;

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get(typeList[i] + "_op", "=");
            string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
            float value = Parser.ParseEffectOperation(cmpValue, effect, buffUnit, rhsUnit);

            if (!Operator.Condition(op, buff.GetBuffIdentifier(typeList[i]), value))
                return false;
        }
        return true;
    }

    public static bool SkillCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        string type = condOptions.Get("type", "none");
        string[] typeList = type.Split('/');

        var invokeUnitId = ((Unit)effect.invokeUnit).id;
        var skillState = (effect.condition == EffectCondition.LastSkill) ? state.lastTurnState : state;

        if (skillState == null)
            return bool.Parse(condOptions.Get("default", "false"));

        Unit skillUnit = (who == "me") ? skillState.GetUnitById(invokeUnitId) : skillState.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = skillState.GetRhsUnitById(skillUnit.id);
        var skillSystem = skillUnit.skillSystem;

        if (type == "none")
            return true;

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get(typeList[i] + "_op", "=");
            string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
            float value = Parser.ParseEffectOperation(cmpValue, effect, skillUnit, rhsUnit);

            if (!Operator.Condition(op, Identifier.GetSkillIdentifier(typeList[i], skillSystem), value))
                return false;
        }
        return true;
    }

    public static bool PokerCondition(this Effect effect, BattleState state, Dictionary<string, string> condOptions) {
        string who = condOptions.Get("who", "me");
        var type = condOptions.Get("type", "none");
        string[] typeList = type.Split('/');
        var invokeUnitId = ((Unit)effect.invokeUnit).id;

        Unit pokerUnit = (who == "me") ? state.GetUnitById(invokeUnitId) :state.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = state.GetRhsUnitById(pokerUnit.id);
        var cards = pokerUnit.pet.buffController.GetRangeBuff(x => x.info.options.Get("group") == "poker");
        
        if (type == "none")
            return true;

        float GetPokerIdentifier(string code) {
            if (code.TryTrimStart("straight", out _) && code.TryTrimParentheses(out var countExpr) 
                && int.TryParse(countExpr, out var count)) 
            {
                var sortByPoint = cards.Select(x => x.value).Distinct().OrderBy(x => x).ToList();
                int goal = 1;
                for (int i = 1; i < sortByPoint.Count; i++) {
                    if (sortByPoint[i] - sortByPoint[i - 1] == 1)
                        goal++;
                    else
                        goal = 1;

                    if (goal >= count)
                        return 1;
                }
                return 0;
            }

            switch (code) {
                default:
                    return 0;
                case "count":
                    return cards.Count;
                case "color.same.max.count":
                    return cards.GroupBy(x => x.id).Max(x => x.Count());
                case "point.same.max.count":
                    return cards.GroupBy(x => x.value).Max(x => x.Count());
            }
        }

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get(typeList[i] + "_op", "=");
            string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
            float value = Parser.ParseEffectOperation(cmpValue, effect, pokerUnit, rhsUnit);
            if (!Operator.Condition(op, GetPokerIdentifier(typeList[i]), value))
                return false;
        }

        return true;
    }
}

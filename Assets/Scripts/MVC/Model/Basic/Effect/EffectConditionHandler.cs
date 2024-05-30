using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if ((state.phase != EffectTiming.OnAttack) && (state.phase != EffectTiming.OnAfterAttack))
            return true;

        string who = condOptions.Get("whos_attack", "me");

        if (who == "all")
            return true;

        string hit = condOptions.Get("is_hit", "true");
        string move = condOptions.Get("is_move", "true");
        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);

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
        string op = condOptions.Get("random_op", "LTE");
        string cmpValue = condOptions.Get("random_cmp", "100");

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

            if (!Operator.Condition(op, Identifier.GetPetIdentifier(typeList[i], battlePet), value))
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

        int buffId; bool ownBuff; BuffType buffType;
        if (!bool.TryParse(own, out ownBuff))
            return false;

        var invokeUnitId = ((Unit)effect.invokeUnit).id;
        Unit buffUnit = (who == "me") ? state.GetUnitById(invokeUnitId) : state.GetRhsUnitById(invokeUnitId);
        Unit rhsUnit = state.GetRhsUnitById(buffUnit.id);
        var pet = buffUnit.pet;

        if (!int.TryParse(id, out buffId)) {
            buffType = id.ToBuffType();
            if (buffType == BuffType.None)
                return false;

            return ownBuff != List.IsNullOrEmpty((buffType == BuffType.TurnBased) ? 
                pet.buffController.GetRangeBuff(x => x.turn > 0) :
                pet.buffController.GetRangeBuff(x => x.info.type == buffType));
        }

        var buff = string.IsNullOrEmpty(key) ? pet.buffController.GetBuff(buffId) : state.stateBuffs.Find(x => x.Key == key).Value;
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
        var skillSystem = skillUnit.skillSystem;

        if (type == "none")
            return true;

        for (int i = 0; i < typeList.Length; i++) {
            string op = condOptions.Get(typeList[i] + "_op", "=");
            string cmpValue = condOptions.Get(typeList[i] + "_cmp", "0");
            float value;
            if (!float.TryParse(cmpValue, out value))
                return false;

            if (!Operator.Condition(op, Identifier.GetSkillIdentifier(typeList[i], skillSystem), value))
                return false;
        }
        return true;
    }
}

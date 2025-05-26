using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EffectAbilityHandler
{
    public static Player player => Player.instance;
    public static Battle battle => player.currentBattle;

    public static bool Win(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        string who = effect.abilityOptionDict.Get("who", "me");
        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);

        state.phase = EffectTiming.OnBattleEnd;
        state.result.state = lhsUnit.IsMyUnit() ? BattleResultState.Win : BattleResultState.Lose;
        state.myUnit.isDone = state.opUnit.isDone = true;
        state.actionCursor = state.actionOrder.Count;
        return true;
    }

    public static bool Escape(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        state.phase = EffectTiming.OnBattleEnd;
        state.result.state = state.atkUnit.IsMyUnit() ? BattleResultState.MyEscape : BattleResultState.OpEscape;
        state.myUnit.isDone = state.opUnit.isDone = true;
        state.actionCursor = state.actionOrder.Count;
        return true;
    }

    public static bool Capture(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        string prob = effect.abilityOptionDict.Get("prob", "0");
        if (!float.TryParse(prob, out float value))
            return false;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = state.GetUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        float lostPercent = 1 - (rhsUnit.pet.hp * 1f / rhsUnit.pet.maxHp);
        if (value < 100) {
            if (rhsUnit.pet.buffController.GetBuff(-2) != null)
                value /= 3f;

            if (lostPercent <= 0.25f)
                value *= 0.25f;
            else if (lostPercent <= 0.5f)
                value *= 1f;
            else if (lostPercent <= 0.75f)
                value *= 1.5f;
            else
                value *= 2f;
        }

        bool isSuccess = (rhsUnit.pet.basic.baseId == 10) || (value >= lhsUnit.RNG());
        lhsUnit.skill.options.Set("capture_result", isSuccess ? "true" : "false");
        
        if (isSuccess) {
            state.phase = EffectTiming.OnBattleEnd;
            state.result.state = BattleResultState.CaptureSuccess;
            state.myUnit.isDone = state.opUnit.isDone = true;
            state.actionCursor = state.actionOrder.Count;
        }
        return true;
    }

    public static bool PetChange(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        string who = effect.abilityOptionDict.Get("who", "me");
        string target = effect.abilityOptionDict.Get("target_index", "-1");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit rhsUnit = state.GetRhsUnitById(invokeUnit.id);
        Unit petChangeUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : rhsUnit;

        // Check pet bag index.
        var randomIndexList = petChangeUnit.petSystem.petBag.FindAllIndex(x => (x != null) && (x != petChangeUnit.pet) && (!x.isDead)).ToList();
        if ((target == "random") && (randomIndexList.Count == 0))
            return false;

        int targetIndex = (target == "random") ? randomIndexList.Random() : (int)(Parser.ParseEffectOperation(target, effect, invokeUnit, rhsUnit));
        if (targetIndex < 0)
            return false;

        // Inherit Buffs: 主動換場繼承
        // Legacy Buffs: 死亡繼承
        bool isDead = petChangeUnit.pet.isDead;
        float anger = petChangeUnit.pet.anger;
        float petSendAngerRatio = (petChangeUnit.pet.buffController.GetBuff(63)?.value ?? 80) / 100f;
        var inheritBuffs = petChangeUnit.pet.buffs.FindAll(x => x.info.inherit);
        var legacyBuffs = petChangeUnit.pet.buffs.FindAll(x => x.info.legacy);
        var tmpPhase = state.phase;

        // On before pet change
        state.phase = EffectTiming.OnBeforePetChange;
        state.ApplyBuffs();
        state.phase = tmpPhase;

        // Remove buffs that dont keep when change pet. Refresh stayTurn of all pet to 0.
        petChangeUnit.pet.buffController.RemoveRangeBuff(x => !x.info.keep, petChangeUnit, state);
        petChangeUnit.petSystem.cursor = targetIndex;
        petChangeUnit.petSystem.RefreshStayTurn();

        // Set anger to 80% original anger. Inherit buffs according to dead or not.
        float petReceiveAngerRatio = (petChangeUnit.pet.buffController.GetBuff(64)?.value ?? 100) / 100f;
        petChangeUnit.pet.anger = Mathf.FloorToInt(anger * petSendAngerRatio * petReceiveAngerRatio);
        petChangeUnit.pet.buffController.AddRangeBuff((isDead ? legacyBuffs : inheritBuffs), petChangeUnit, state);

        // Record that this pet has participated in fight.
        if (petChangeUnit.id == state.myUnit.id)
            state.result.AddFightPetCursor(targetIndex);

        // On after pet change
        state.phase = EffectTiming.OnAfterPetChange;
        state.ApplyBuffs();
        state.phase = tmpPhase;
        
        return true;
    }

    public static bool Heal(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "skill");
        string add = effect.abilityOptionDict.Get("add", "0");
        string set = effect.abilityOptionDict.Get("set","none");
        string max = effect.abilityOptionDict.Get("max","none");
        
        if (state == null) {
            Pet healPet = (Pet)effect.invokeUnit;
            var setHp = Parser.ParseEffectOperation((set == "none") ? add : set, effect, null, null, healPet);
            if (set == "none")
                setHp += healPet.currentStatus.hp;

            healPet.currentStatus.hp = Mathf.Clamp((int)setHp, 0, healPet.normalStatus.hp);
            return true;
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        List<BattlePet> targetList = Parser.GetBattlePetTargetList(state, effect, lhsUnit, rhsUnit);

        for (int i = 0; i < targetList.Count; i++) {
            var heal = Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit, targetList[i]);
            bool lostToGain = (heal < 0) && (targetList[i].buffController.GetBuff(93) != null);
            bool gainToLost = (heal > 0) && (targetList[i].buffController.GetBuff(1020) != null);
            if (lostToGain || gainToLost)
                heal *= -1;

            var rec = targetList[i].battleStatus.rec;
            if (targetList[i].buffController.GetBuff(100) != null)
                rec = Mathf.Max(rec, 100);

            int healAdd = (int)(heal * ((heal > 0) ? (rec / 100f) : 1));
            var setHp = (set == "none") ? 0 : (int)Parser.ParseEffectOperation(set, effect, lhsUnit, rhsUnit, targetList[i]);
            var maxHp = (max == "none") ? 0 : (int)Parser.ParseEffectOperation(max, effect, lhsUnit, rhsUnit, targetList[i]);

            if (set != "none") {
                if ((setHp <= 0) && (!ListHelper.IsNullOrEmpty(targetList[i].buffController.GetRangeBuff(x => (x.id == 99) || (x.id == -8)))))
                    continue;

                targetList[i].hp = setHp;
                continue;
            }

            if (max != "none") {
                if ((maxHp <= 0) && (!ListHelper.IsNullOrEmpty(targetList[i].buffController.GetRangeBuff(x => (x.id == 99) || (x.id == -8)))))
                    continue;

                if ((maxHp < targetList[i].maxHp) && (!ListHelper.IsNullOrEmpty(targetList[i].buffController.GetRangeBuff(x => (x.id == -7) || (x.id == -8)))))
                    continue;

                targetList[i].maxHp = maxHp;
                continue;
            }

            // If already dead, no heal.
            if (targetList[i].isDead)
                continue;

            if ((type == "item") && (who == "me")) {
                lhsUnit.skillSystem.itemHeal += healAdd;
            } else if ((type == "skill") && (who == "me")) {
                lhsUnit.skillSystem.skillHeal += healAdd;
            } else if (type == "buff") {
                lhsUnit.skillSystem.buffHeal += healAdd;
                healAdd = 0;
            }

            targetList[i].hp += healAdd;
        }

        return true;
    }

    public static bool Rage(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string mult = effect.abilityOptionDict.Get("mult", "0/1");
        string add = effect.abilityOptionDict.Get("add", "0");
        string set = effect.abilityOptionDict.Get("set", "none");
        string min = effect.abilityOptionDict.Get("min", "none");
        string max = effect.abilityOptionDict.Get("max", "none");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        var statusController = lhsUnit.pet.statusController;
        statusController.minAnger = (min == "none") ? statusController.minAnger : (int)Parser.ParseEffectOperation(min, effect, lhsUnit, rhsUnit);
        statusController.maxAnger = (max == "none") ? statusController.maxAnger : (int)Parser.ParseEffectOperation(max, effect, lhsUnit, rhsUnit);

        if (set == "none") {
            float anger = Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit);
            int angerAdd = (int)(anger * ((anger > 0) ? (lhsUnit.pet.battleStatus.angrec / 100f) : 1));
            lhsUnit.pet.anger += angerAdd;
        } else {
            float anger = Parser.ParseEffectOperation(set, effect, lhsUnit, rhsUnit);
            lhsUnit.pet.anger = (int)anger;
        }
        return true;
    }

    //! Note that currently block powerup/down should be together with clear powerup/down
    public static bool Powerup(this Effect effect, BattleState state) {
        Status status = new Status();
        string[] typeNames = Status.typeNames.Update("hp", "hit").ToArray();

        string who = effect.abilityOptionDict.Get("who", "me");
        string random = effect.abilityOptionDict.Get("random", "false");
        string opCopy = effect.abilityOptionDict.Get("copy", "false");
        string min = effect.abilityOptionDict.Get("min", "none");
        string max = effect.abilityOptionDict.Get("max", "none");

        bool isRandom = false, isOpCopy = false;
        if ((!bool.TryParse(random, out isRandom)) || ((opCopy != "reverse") && (!bool.TryParse(opCopy, out isOpCopy))))
            return false;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        List<BattlePet> targetList = Parser.GetBattlePetTargetList(state, effect, lhsUnit, rhsUnit);

        for (int j = 0; j < targetList.Count; j++) {
            var pet = targetList[j];
            var statusController = pet.statusController;
            var buffController = pet.buffController;

            // Prepare Powerup Status
            for (int type = 0; type < typeNames.Length; type++) {
                string add = effect.abilityOptionDict.Get(typeNames[type], "0");
                status[type] = Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit, pet);
            }

            if (isRandom) {
                string pdf = effect.abilityOptionDict.Get("random_pdf", "none");
                string randomTypeCountExpr = effect.abilityOptionDict.Get("random_count", "1");
                int count = status.Count(x => x != 0);
                int randomTypeCount = (int)Parser.ParseEffectOperation(randomTypeCountExpr, effect, lhsUnit, rhsUnit, targetList[j]);

                if (pdf == "none") {
                    // pass.
                } else if (pdf == "uniform") {
                    // Uniformly random can have 2 or more different types powerup.
                    var typeList = Enumerable.Range(0, count).ToList().Random(randomTypeCount, false);
                    for (int i = 0, index = 0; i < typeNames.Length; i++) {
                        if (status[i] == 0)
                            continue;

                        status[i] = typeList.Contains(index) ? status[i] : 0;
                        index++;
                    }
                } else {
                    // Custom pdf cannot have 2 or more different types powerup.
                    var probList = pdf.ToFloatList('/');
                    var sum = probList.Sum();
                    float rng = Random.Range(0f, sum);
                    int type = 0;
                    for (type = 0; type < probList.Count; type++) {
                        if (probList[type] >= rng)
                            break;

                        rng -= probList[type];
                    }
                    for (int i = 0, index = 0; i < 5; i++) {
                        if (status[i] == 0)
                            continue;

                        status[i] = (index == type) ? status[i] : 0;
                        index++;
                    }
                }
            }

            // Handle min and max powerup.
            if (min != "none") {
                var minPowerup = (int)Parser.ParseEffectOperation(min, effect, lhsUnit, rhsUnit, targetList[j]);
                pet.statusController.SetMinPowerUp(minPowerup * Status.one);
            } 
            
            if (max != "none") {
                var maxPowerup = (int)Parser.ParseEffectOperation(max, effect, lhsUnit, rhsUnit, targetList[j]);
                pet.statusController.SetMaxPowerUp(maxPowerup * Status.one);
            }

            // Powerup.
            if (buffController.GetBuff(45) != null)
                status = status.Select(x => x * (x > 0 ? 2 : 1));

            if (buffController.GetBuff(46) != null)
                status = status.Select(x => x * (x < 0 ? 2 : 1));

            if (buffController.GetBuff(94) != null)
                status *= -1;

            pet.PowerUp(status, lhsUnit, state);    
        }

        if (opCopy == "reverse") {
            rhsUnit.pet.PowerUp(-1 * status, rhsUnit, state);
            return true;
        } 

        if (isOpCopy) {
            rhsUnit.pet.PowerUp(status, rhsUnit, state);
            return true;
        }

        return true;
    }

    //! Note that add hp wont work, since it should be heal.
    //! Note that add anger wont work, since it should be rage.
    public static bool AddStatus(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "5");
        string mult = effect.abilityOptionDict.Get("mult", "0/1");
        string add = effect.abilityOptionDict.Get("add", "0");

        var typeList = type.ToIntList('/');
        if (ListHelper.IsNullOrEmpty(typeList))
            return false;

        for (int i = 0; i < typeList.Count; i++) {
            int statusType = typeList[i];

            Unit invokeUnit = (Unit)effect.invokeUnit;
            Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
            Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
            var pet = lhsUnit.pet;
            float statusMult = Parser.ParseEffectOperation(mult, effect, lhsUnit, rhsUnit);
            int statusAdd = (int)Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit);

            if (statusMult != 0)
                pet.statusController.MultCurrentStatus(statusType, statusMult);

            //statusAdd += Mathf.FloorToInt(pet.initStatus[statusType] * statusMult);
            pet.statusController.AddBattleStatus(statusType, statusAdd);
        }

        return true;
    }

    public static bool BlockOrCopyBuff(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string op = effect.abilityOptionDict.Get("op", "+");
        string idList = effect.abilityOptionDict.Get("id_list", "0");
        string typeList = effect.abilityOptionDict.Get("type_list", "none");

        List<int> idBlockList = idList.ToIntList('/');
        List<BuffType> typeBlockList = typeList.Split('/').Select(x => x.ToBuffType()).ToList();

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        var statusController = lhsUnit.pet.statusController;
        var buffController = lhsUnit.pet.buffController;

        Action<List<int>> idFunc = op switch {
            "+" => buffController.BlockBuff,
            "-" => buffController.UnblockBuff,
            "*" => buffController.CopyBuff,
            "/" => buffController.UncopyBuff,
            _ => (idList) => {},
        };
        
        Action<List<BuffType>> typeFunc = op switch {
            "+" => buffController.BlockBuffWithType,
            "-" => buffController.UnblockBuffWithType,
            "*" => buffController.CopyBuffWithType,
            "/" => buffController.UncopyBuffWithType,
            _ => (typeList) => {},
        };

        idFunc.Invoke(idBlockList);
        typeFunc.Invoke(typeBlockList);
        return true;
    }

    public static bool AddBuff(this Effect effect, BattleState state) {
        string key = effect.abilityOptionDict.Get("key", string.Empty);
        string who = effect.abilityOptionDict.Get("who", "me");
        string id = effect.abilityOptionDict.Get("id", "0");
        string turn = effect.abilityOptionDict.Get("turn", "-1");
        string value = effect.abilityOptionDict.Get("value", "0");
        string option = effect.abilityOptionDict.Get("option", string.Empty);

        var idExpr = id.TryTrimStart("!", out var idListExpr) ? idListExpr.Substring(1, idListExpr.Length - 2).Split('/') : new string[]{ id };
        var turnExpr = turn.TryTrimStart("!", out var turnListExpr) ? turnListExpr.Substring(1, turnListExpr.Length - 2).Split('/') : new string[]{ turn };
        var valueExpr = value.TryTrimStart("!", out var valueListExpr) ? valueListExpr.Substring(1, valueListExpr.Length - 2).Split('/') : new string[]{ value };
        var optionExpr = option.TryTrimStart("!", out var optionListExpr) ? optionListExpr.Substring(1, optionListExpr.Length - 2).Split('/') : new string[]{ option };
        int buffCount = idExpr.Length;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        List<BattlePet> targetList = Parser.GetBattlePetTargetList(state, effect, lhsUnit, rhsUnit);
        bool isFieldLocked = (lhsUnit.pet.buffController.GetBuff(92) != null) || (rhsUnit.pet.buffController.GetBuff(92) != null);

        bool isSuccess = false;

        for (int j = 0; j < targetList.Count; j++)
        {
            var pet = targetList[j];
            var buffController = targetList[j].buffController;

            for (int k = 0; k < buffCount; k++)
            {
                int buffTurn = (int)Parser.ParseEffectOperation(turnExpr[k], effect, lhsUnit, rhsUnit);
                int buffValue = (int)Parser.ParseEffectOperation(valueExpr[k], effect, lhsUnit, rhsUnit);
                int buffId = 0;
                var buffIdList = new List<int>();

                if (idExpr[k].TryTrimStart("unique", out var trimId))
                {
                    buffIdList = trimId.ToIntRange();
                    if (!string.IsNullOrEmpty(key) || (buffIdList.Exists(x => Buff.GetBuffInfo(x) == null)))
                        return false;
                    buffIdList = buffIdList.Where(x => !buffController.buffs.Exists(y => y.id == x)).ToList();
                    if (ListHelper.IsNullOrEmpty(buffIdList))
                        return false;
                    buffId = buffIdList.Random();
                }
                else
                {
                    if (idExpr[k].TryTrimStart("random", out trimId) && trimId.TryTrimParentheses(out var buffType) && (buffType.ToBuffType() != BuffType.None))
                        buffId = Database.instance.buffInfoDict.Where(entry => entry.Value.type == buffType.ToBuffType()).Select(entry => entry.Key).ToList().Random();
                    else
                        buffId = (int)Parser.ParseEffectOperation(idExpr[k], effect, lhsUnit, rhsUnit);
                }
                var newBuffInfo = Buff.GetBuffInfo(buffId);
                if (string.IsNullOrEmpty(key) && (newBuffInfo == null))
                    return false;

                // The key prefix "rule" is reserved for pvp rules.
                Buff newBuff = (newBuffInfo == null) ? null : new Buff(buffId, buffTurn, buffValue);
                if (!string.IsNullOrEmpty(optionExpr.ElementAtOrDefault(k)))
                {
                    var optionList = optionExpr[k].TrimParenthesesLoop('(', ')');
                    if (!ListHelper.IsNullOrEmpty(optionList))
                    {
                        foreach (var op in optionList)
                            newBuff.options.Set(op.Split(':')[0], op.Split(':')[1]);
                    }
                }

                if (!string.IsNullOrEmpty(key))
                {
                    if ((isFieldLocked && (key == "field")) || key.StartsWith("rule"))
                        return false;

                    if (key == "unit")
                        return lhsUnit.AddBuff(newBuff);

                    state.stateBuffs.RemoveAll(x => x.Key == key);
                    if (newBuff != null)
                        state.stateBuffs.Add(new KeyValuePair<string, Buff>(key, newBuff));

                    return true;
                }

                isSuccess |= buffController.AddBuff(newBuff, lhsUnit, state);
            }
        }
        return isSuccess;
    }

    public static bool RemoveBuff(this Effect effect, BattleState state) {
        string keyList = effect.abilityOptionDict.Get("key", string.Empty);
        string who = effect.abilityOptionDict.Get("who", "me");
        string idList = effect.abilityOptionDict.Get("id", "0");
        string typeList = effect.abilityOptionDict.Get("type", "none");
        string filterList = effect.abilityOptionDict.Get("filter", "none");

        // The key prefix "rule" is reserved for pvp rules.
        string[] keyRange = keyList.Split('/').Where(x => !x.StartsWith("rule")).ToArray();
        List<int> idRange = idList.ToIntList('/');
        string[] typeRange = typeList.Split('/');

        bool isKey = !string.IsNullOrEmpty(keyList);
        bool isId = (idList != "0") && (idRange.Count != 0);
        bool isType = typeList != "none";

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        var buffController = lhsUnit.pet.buffController;
        var statusController = lhsUnit.pet.statusController;
        var filter = Parser.ParseConditionFilter<Buff>(filterList, (id, buff) => {
            if (buff == null)
                return float.MinValue;

            return Parser.ParseEffectOperation(id, effect, lhsUnit, rhsUnit, buff, false);
        });

        if (isKey) {
            // Unit Buffs
            if (keyList == "unit") {
                bool isSuccess = false;
                if (isType) {
                    foreach (var type in typeRange)
                        isSuccess |= lhsUnit.RemoveBuff(x => (!x.IsUneffectable()) && x.IsType(type.ToBuffType()) && filter(x));

                    return isSuccess;
                }
                if (isId)
                    return lhsUnit.RemoveBuff(x => idRange.Contains(x.id) && filter(x));
                    
                return false;
            }
            // State Buffs
            bool isFieldLocked = (lhsUnit.pet.buffController.GetBuff(92) != null) || (rhsUnit.pet.buffController.GetBuff(92) != null);
            Func<string, bool> fieldLockFilter = (key) => !(isFieldLocked && (key == "field"));
            return state.stateBuffs.RemoveAll(x => keyRange.Contains(x.Key) && filter(x.Value) && fieldLockFilter(x.Key)) > 0;
        }

        if (isType) {
            var isSuccess = false;
            foreach (var type in typeRange) {
                var buffType = type.ToBuffType();
                isSuccess |= buffController.RemoveRangeBuff(x => (!x.IsUneffectable()) && x.IsType(buffType) && filter(x), lhsUnit, state);
            }
            return isSuccess;
        }
        if (isId) {
            foreach (var id in idRange) {
                var buff = new Buff(id);
                if ((!buff.IsPower()) || (!filter(buff)))
                    continue;

                int buffId = id.IsWithin(-9, -10) ? 5 : ((id - 1) / 2);

                // id % 2 == 1 => want to clear powerup
                // powerup < 0 => current is powerdown
                // ^ is xor operator, which returns false when both true or both false
                if ((Mathf.Abs(id) % 2 == 1) ^ (statusController.powerup[buffId] < 0))
                    statusController.SetPowerUp(buffId, 0);
            }
            return buffController.RemoveRangeBuff(x => idRange.Contains(x.id) && filter(x), lhsUnit, state);
        }

        return false;
    }

    public static bool CopyBuff(this Effect effect, BattleState state) {
        string idList = effect.abilityOptionDict.Get("id", "0");
        string typeList = effect.abilityOptionDict.Get("type", "none");
        string filterList = effect.abilityOptionDict.Get("filter", "none");
        string source = effect.abilityOptionDict.Get("source", "op");
        string target = effect.abilityOptionDict.Get("target", "me");
        string transfer = effect.abilityOptionDict.Get("transfer", "false");
        string reverse = effect.abilityOptionDict.Get("reverse", "false");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (source == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = (target == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        var lhsBuffController = lhsUnit.pet.buffController;
        var rhsBuffController = rhsUnit.pet.buffController;
        var copyType = typeList.ToBuffType();
        var filter = Parser.ParseConditionFilter<Buff>(filterList, (id, buff) => {
            if (buff == null)
                return float.MinValue;

            return Parser.ParseEffectOperation(id, effect, lhsUnit, rhsUnit, buff, false);
        });
        IEnumerable<Buff> buffs = null;

        List<int> GetRandomBuff(bool unique = false) {
            var all = lhsBuffController.GetRangeBuff(x => (!x.info.hide) && (!x.IsUneffectable()) &&
                (!x.IsPower()) && (!x.IsType(BuffType.Item)) && filter(x))
                .Select(x => x.id).ToList();

            if (unique) 
                all.RemoveAll(id => rhsBuffController.GetBuff(id) != null);
            
            if (ListHelper.IsNullOrEmpty(all))
                return new List<int>();

            return all.Random(1);
        }

        List<int> idRange = idList switch {
            "random"            => GetRandomBuff(false),
            "unique[random]"    => GetRandomBuff(true),
            _                   => idList.ToIntList('/'),
        };
        string[] typeRange = typeList.Split('/');
        bool isId = (idList != "0") && (idRange.Count != 0);
        bool isType = (typeList != "none");

        if (!isId && !isType)
            return false;

        if (!bool.TryParse(transfer, out bool isTransfer) || !bool.TryParse(reverse, out bool isReverse))
            return false;

        if (isId) {
            buffs = lhsBuffController.GetRangeBuff(x => idRange.Contains(x.id) && filter(x));
            var powerup = lhsUnit.pet.statusController.powerup;
            var status = new Status();
            foreach (var buff in buffs) {
                if (!buff.IsPower()) {
                    rhsBuffController.AddBuff(new Buff(buff), rhsUnit, state);
                    continue;
                }
                
                int type = buff.id.IsWithin(-9, -10) ? 5 : (buff.id - 1) / 2;
                status[type] = (isReverse ? -1 : 1) * powerup[type];

                if (isTransfer || isReverse) {
                    lhsUnit.pet.statusController.SetPowerUp(type, 0);
                }
            }
            rhsUnit.pet.PowerUp(status, rhsUnit, state);
        }
        if (isType) {
            buffs = lhsBuffController.GetRangeBuff(x => x.IsType(copyType) && filter(x));
            rhsBuffController.AddRangeBuff(buffs.Select(x => new Buff(x)), rhsUnit, state);
        }

        if (isTransfer) {
            lhsBuffController.RemoveRangeBuff(buffs, lhsUnit, state);
        }
        return !ListHelper.IsNullOrEmpty(buffs);
    }

    public static bool SetBuff(this Effect effect, BattleState state) {
        string key = effect.abilityOptionDict.Get("key", string.Empty);
        string filterList = effect.abilityOptionDict.Get("filter", "none");
        string who = effect.abilityOptionDict.Get("who", "me");
        string id = effect.abilityOptionDict.Get("id", "0");
        string type = effect.abilityOptionDict.Get("type", "value");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");

        if (!int.TryParse(id, out var buffId))
            return false;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        var filter = Parser.ParseConditionFilter<Buff>(filterList, (id, buff) => {
            if (buff == null)
                return float.MinValue;

            return Parser.ParseEffectOperation(id, effect, lhsUnit, rhsUnit, buff, false);
        });

        var buffList = new List<Buff>();
        if (string.IsNullOrEmpty(key))
            buffList = lhsUnit.pet.buffController.GetRangeBuff(x => filter(x) && ((x.id == buffId) || (buffId == 0)));
        else if (key == "unit")
            buffList = lhsUnit.unitBuffs.FindAll(x => filter(x) && (x.id == buffId));
        else
            buffList = state.stateBuffs.FindAll(x => x.Key == key).Select(x => x.Value).ToList();
            
        var success = false;
        foreach (var buff in buffList) {
            if (buff == null)
                continue;
            
            float oldValue = buff.GetBuffIdentifier(type);
            float newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
            buff.SetBuffIdentifier(type, Operator.Operate(op, oldValue, newValue));
            if (buff.info.autoRemove && buff.value <= 0) {
                if (string.IsNullOrEmpty(key))
                    lhsUnit.pet.buffController.RemoveBuff(buff, lhsUnit, state);
                else if (key == "unit")
                    lhsUnit.unitBuffs.RemoveAll(x => x == buff);
                else
                    state.stateBuffs.RemoveAll(x => x.Value == buff);
            }
            success = true;
        }
        return success;
    }

    public static bool SetDamage(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string turn = effect.abilityOptionDict.Get("state", "current");
        string type = effect.abilityOptionDict.Get("type", "skill");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");

        BattleState damageState = turn switch {
            "last" => state.lastTurnState,
            _ => state,
        };

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit damageUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : damageState.GetRhsUnitById(invokeUnit.id);
        var skillSystem = damageUnit.skillSystem;
        float damage = 0;

        if (damageState != null) {
            Unit lhsUnit = damageState.GetUnitById(invokeUnit.id);
            Unit rhsUnit = damageState.GetRhsUnitById(lhsUnit.id);
            damage = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
        }

        if ((state.phase == EffectTiming.OnTurnEnd) && UnitSkillSystem.normalHpType.Contains(type)) {
            skillSystem.buffDamage = (int)Operator.Operate(op, skillSystem.buffDamage, damage);
        } else if (type == "skill") {
            skillSystem.skillDamage = (int)Operator.Operate(op, skillSystem.skillDamage, damage);
        } else if (type == "item") {
            skillSystem.itemDamage = (int)Operator.Operate(op, skillSystem.itemDamage, damage);
        } else {
            skillSystem.damageDict[type] = (int)Operator.Operate(op, skillSystem.damageDict.Get(type, 0), damage);
        }

        return true;
    }

    public static bool SetSkill(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "none");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        if (type == "id") {
            string setAnger = effect.abilityOptionDict.Get("set_anger", "false");
            string resetParam = effect.abilityOptionDict.Get("reset_param", "false");
            if ((!bool.TryParse(setAnger, out bool isSetAnger)) || (!bool.TryParse(resetParam, out bool isResetParam)))
                return false;

            Skill newSkill = new Skill(value switch {
                "-1" => Skill.GetNoOpSkill(),
                "-3" => Skill.GetPetChangeSkillWithTiming(lhsUnit.skill, EffectTiming.OnTurnEnd),
                "-4" => Skill.GetEscapeSkill(),
                "random" => Skill.GetRandomSkill(),
                "random[available]" => lhsUnit.pet.skillController.GetAvailableSkills(lhsUnit.pet.anger).Random(),
                _ => Skill.GetSkill((int)Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit)),
            });

            if (!isSetAnger)
                newSkill.anger = lhsUnit.skill.anger;

            lhsUnit.skillSystem.skill = newSkill;

            if (isResetParam)
                lhsUnit.skillSystem.PrepareDamageParam(state.atkUnit.pet, state.defUnit.pet);

            return true;
        }

        if (type == "effect") {
            string randomTypeCountExpr = effect.abilityOptionDict.Get("random_count", "1");
            int randomTypeCount = (int)Parser.ParseEffectOperation(randomTypeCountExpr, effect, lhsUnit, rhsUnit);

            var skillIdList = value.Split('/').Select(x => (int)Parser.ParseEffectOperation(x, effect, lhsUnit, rhsUnit)).ToList();
            skillIdList = skillIdList.Random(randomTypeCount, false);

            if ((!ListHelper.IsNullOrEmpty(skillIdList)) && (skillIdList.Count > 1)) {
                var effectList = new List<Effect>();
                skillIdList.ForEach(skillId => effectList.AddRange(Skill.GetSkill(skillId, false)?.effects
                    .Select(x => new Effect(x)) ?? new List<Effect>()));
                lhsUnit.skillSystem.skill?.SetEffects(effectList);
                return true;
            }
        }

        var skill = lhsUnit.skillSystem;
        float oldValue = Identifier.GetSkillIdentifier(type, skill);
        float newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
        skill.SetSkillSystemIdentifier(type, Operator.Operate(op, oldValue, newValue));

        if ((battle.settings.parallelCount > 1) && (type == "parallelTargetIndex"))
            rhsUnit.petSystem.cursor = (int)(skill.skill?.GetSkillIdentifier("parallelTargetIndex") ?? 0);
            
        return true;
    }

    public static bool SetPet(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "none");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");
        float oldValue, newValue;

        if (state == null) {
            if (effect.target == EffectTarget.CurrentPetBag) {
                var isSuccess = false;
                var petBagMode = (PetBagMode)int.Parse(effect.abilityOptionDict.Get("pet_bag_mode", "0"));
                IEnumerable<Pet> petBag = petBagMode switch {
                    PetBagMode.Normal   => Player.instance.petBag,
                    PetBagMode.YiTeRogue=> YiTeRogueData.instance.petBag,
                    _ =>  ((Pet)effect.invokeUnit).SingleToList(),
                };
                foreach (var pet in petBag) {
                    if (pet == null)
                        continue;
                        
                    isSuccess |= ResidentSetPet(pet);
                }

                SaveSystem.SaveData();
                return isSuccess;
            }
            var result = ResidentSetPet((Pet)effect.invokeUnit);
            SaveSystem.SaveData();
            return result;
            
            bool ResidentSetPet(Pet pet) 
            {
                oldValue = pet.GetPetIdentifier(type);
                newValue = pet.TryGetPetIdentifier(value, out var num) ? num : Identifier.GetNumIdentifier(value);
                // Evolve pet.
                if ((type == "id") && (op == "SET")) {
                    var evolveId = (int)newValue;
                    if (Pet.GetPetInfo(evolveId) == null)
                        return false;
                    string keepSkillExpr = effect.abilityOptionDict.Get("keep_skill", "true");
                    if (!bool.TryParse(keepSkillExpr, out var keepSkill))
                        return false;

                    pet.EvolveTo(evolveId, keepSkill);
                    return true;
                }
                // Add Skin.
                if ((type == "skinId") && (op == "+")) {
                    var newSkinId = value.ToIntList('/');
                    pet.ui.specialSkinList.AddRange(newSkinId.Where(id => Pet.GetPetInfo(id) != null));
                    return true;
                }
                // Reset ev.
                if (type == "evReset") {
                    pet.talent.ResetEV();
                    return true;
                }
                // Set personality.
                if ((type == "personality") && ((value == "-1") || (value == "-2"))) {
                    var petBagController = GameObject.FindObjectOfType<PetBagController>();
                    if (petBagController == null)
                        return false;

                    petBagController.SetPetPersonality(int.Parse(value));
                    return true;
                }
                // Learn skill.
                if (type == "skill") {
                    if (!int.TryParse(value, out var skillId))
                        return false;

                    var skill = Skill.GetSkill(skillId, false);
                    if (skill == null)
                        return false;

                    if (op == "+") {
                        if (!pet.skills.LearnNewSkill(skill))
                            return false;
                    } else if (op == "-") {
                        if (!pet.skills.ownSkillId.Contains(skill.id))
                            return false;

                        var backupSuperSkill = pet.backupSuperSkill;
                        pet.skills.ownSkill = pet.ownSkill.Where(x => x.id != skill.id).ToList();
                        if (pet.normalSkill.Any(x => x.id == skill.id))
                            pet.skills.normalSkillId.Update(skill.id, pet.backupNormalSkill.FirstOrDefault()?.id ?? 0);

                        if ((pet.superSkill != null) && (pet.superSkill.id == skill.id))
                            pet.superSkill = backupSuperSkill;
                    }
                    return true;
                }
                if (type == "buff") {
                    if (!int.TryParse(value, out var buffId))
                        return false;

                    if (op == "+")
                        pet.feature.afterwardBuffIds.Add(buffId);
                    else
                        pet.feature.afterwardBuffIds.Remove(buffId);

                    return true;
                }   
                pet.SetPetIdentifier(type, Operator.Operate(op, oldValue, newValue));
                return true;
            }
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        BattlePet battlePet = lhsUnit.pet;

        oldValue = battlePet.GetPetIdentifier(type);
        newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);

        bool TryGetShiftedSkills(BattlePet pet, out Skill[] normalSkills, out Skill superSkill, string normalSkillKey = "normal_skill", string superSkillKey = "super_skill") {
            normalSkills = null;
            superSkill = null;

            var isNormalSkillShift = effect.abilityOptionDict.TryGet(normalSkillKey, out var normalSkillExpr, "none");
            var isSuperSkillShift = effect.abilityOptionDict.TryGet(superSkillKey, out var superSkillExpr, "0");
            if ((!isNormalSkillShift) && (!isSuperSkillShift))
                return false;

            // If normal_skill/super_skill is op, take op's skill.
            // Else if they are shift[COUNT], shift current skill ids with COUNT.
            // Else take it as an int list and get skill ids.
            if (isNormalSkillShift) {
                if (normalSkillExpr == "op") {
                    var opSkillController = rhsUnit.pet.skillController;
                    normalSkills = (ListHelper.IsNullOrEmpty(opSkillController.normalSkills) ? opSkillController.loopSkills : opSkillController.normalSkills)
                        .GroupBy(x => x?.id ?? 0).Select(x => x.First()).Take(4).ToArray();

                    Array.Resize(ref normalSkills, 4);
                } else if (normalSkillExpr.TryTrimStart("shift", out var normalTrim) &&
                    normalTrim.TryTrimParentheses(out var normalShift) &&
                    int.TryParse(normalShift, out var normalShiftCount)) {
                    normalSkills = (ListHelper.IsNullOrEmpty(pet.skillController.normalSkills) ? pet.skillController.loopSkills : pet.skillController.normalSkills)
                        .Where(x => x != null).Select(x => Skill.GetSkill(x.id + normalShiftCount, false)).Take(4).ToArray();
                } else {
                    normalSkills = normalSkillExpr.ToIntList('/').Take(4).Select(id => Skill.GetSkill(id, false)).ToArray();
                }
            }

            if (isSuperSkillShift) {
                if (superSkillExpr == "op") {
                    superSkill = rhsUnit.pet.skillController.superSkill;
                } else if (superSkillExpr.TryTrimStart("shift", out var superTrim) &&
                    superTrim.TryTrimParentheses(out var superShift) &&
                    int.TryParse(superShift, out var superShiftCount)) {
                    superSkill = (pet.skillController.superSkill == null) ? null : (Skill.GetSkill(pet.skillController.superSkill.id + superShiftCount, false));
                } else {
                    var superSkillId = (int)Parser.ParseEffectOperation(superSkillExpr, effect, lhsUnit, rhsUnit);
                    superSkill = Skill.GetSkill(superSkillId, false);
                }
            }
            return true;
        }
        

        // Switch to special pet.
        if (type == "id") {
            if (!bool.TryParse(effect.abilityOptionDict.Get("mod", "false"), out var withMod))
                return false;

            // Special value.
            if (value == "random")
                newValue = Pet.GetRandomPetInfo(withMod).id;

            // Prepare kill list.
            var petSystem = lhsUnit.petSystem;
            var petBagIds = petSystem.petBag.Where(x => (x != null) && (!x.isDead)).Select(x => x.id).ToList();
            var killList = effect.abilityOptionDict.Get("kill", "none").ToIntList('/');

            for (int i = 0; i < killList.Count; i++) {
                if (!petBagIds.Remove(killList[i]))
                    return false;
            }

            for (int i = 0; i < killList.Count; i++) {
                var index = petSystem.petBag.FindIndex(x => (x?.id ?? 0) == killList[i]);
                petSystem.petBag[index].hp = 0;
            }

            // Switch Pet.
            var inheritList = effect.abilityOptionDict.Get("inherit", "buff").Split('/');
            Pet switchPet = new Pet((int)newValue, battlePet);
            bool isSkillShifted = TryGetShiftedSkills(battlePet, out var normalSkills, out var superSkill);
            switchPet.normalSkill = isSkillShifted ? normalSkills : battlePet.normalSkill;
            switchPet.superSkill = isSkillShifted ? superSkill : battlePet.superSkill;

            // Check if inherit skin.
            if (inheritList.Contains("skin"))
                switchPet.ui.skinId = battlePet.ui.skinId;

            // Check if inherit hp.
            var cursor = lhsUnit.petSystem.cursor;
            lhsUnit.petSystem.petBag[cursor] = BattlePet.GetBattlePet(switchPet);
            if (inheritList.Contains("hp"))
                lhsUnit.petSystem.petBag[cursor].hp = battlePet.hp;

            if (inheritList.Contains("anger"))
                lhsUnit.petSystem.petBag[cursor].anger = battlePet.anger;

            lhsUnit.petSystem.petBag[cursor].skillController.loopSkills = lhsUnit.pet.skillController.normalSkills.Where(x => x != null).ToList();
            lhsUnit.petSystem.petBag[cursor].PowerUp(battlePet.statusController.powerup, lhsUnit, state);

            // Add feature and emblem buffs
            List<Buff> buffs = new List<Buff>();
            buffs.Add(Buff.GetFeatureBuff(lhsUnit.pet));
            buffs.Add(Buff.GetEmblemBuff(lhsUnit.pet));
            buffs.AddRange(lhsUnit.pet.initBuffs);

            // Check if inherit buff.
            if (inheritList.Contains("buff")) {
                buffs.AddRange(battlePet.buffController.GetRangeBuff(x => (x != null) && (!x.IsPower()) &&
                    (x.id != Buff.GetFeatureBuff(battlePet).id) &&
                    (x.id != Buff.GetEmblemBuff(battlePet).id) &&
                    (!x.IsUnhealthy()) && (!x.IsAbnormal()) &&
                    (!battlePet.info.ui.defaultBuffIds.Exists(y => x.id == y)) &&
                    (!lhsUnit.pet.initBuffs.Exists(y => x.id == y.id))
                ));
            }

            lhsUnit.pet.buffController.AddRangeBuff(buffs, lhsUnit, state);
            if (lhsUnit.pet.buffController.GetBuff(-7) != null) {
                lhsUnit.pet.maxHp = Mathf.Max(lhsUnit.pet.maxHp, battlePet.maxHp);
                lhsUnit.pet.hp = lhsUnit.pet.maxHp;
                if (!ListHelper.IsNullOrEmpty(battlePet.skillController.loopSkills)) {
                    lhsUnit.pet.skillController.loopSkills = battlePet.skillController.loopSkills;
                    lhsUnit.pet.skillController.superSkill = battlePet.skillController.superSkill;
                }
            }

            return true;
        }

        // Set pet skill.
        if (type.TryTrimStart("skill", out var trimType)) 
        {
            var isSuccess = false;
            var skillController = lhsUnit.pet.skillController;
            var filter = Parser.ParseConditionFilter<Skill>(effect.abilityOptionDict.Get("filter"), 
                (expr, skill) => skill.TryGetSkillIdentifier(expr, out var num) ? num : Identifier.GetNumIdentifier(expr));    
            var skillList = (skillController.normalSkills ?? new List<Skill>()).Append(skillController.superSkill).Where(x => (x != null) && filter(x)).ToList();

            if (trimType.TryTrimParentheses(out var trimSkillExpr)) {
                int trimSkillId = (int)Parser.ParseEffectOperation(trimSkillExpr, effect, lhsUnit, rhsUnit);
                skillList = skillList.Where(x => x.id == trimSkillId).ToList();
                trimType = trimType.TrimStart("[" + trimSkillExpr + "]");
            }

            if (string.IsNullOrEmpty(trimType))
                trimType = ".id";

            trimType = trimType.TrimStart('.');

            for (int i = 0; i < skillList.Count; i++) {
                bool isSuperSkill = ((skillController.superSkill?.id ?? 0) == skillList[i].id);
                int normalSkillIndex = skillController.normalSkills.FindIndex(x => (x?.id ?? 0) == skillList[i].id);
                Skill newSkill = value switch {
                    "-1" => Skill.GetNoOpSkill(),
                    "-4" => Skill.GetEscapeSkill(),
                    "random" => Skill.GetRandomSkill(),
                    _ => Skill.GetSkill((int)newValue, false),
                };

                newSkill = (newSkill == null) ? null : new Skill(newSkill);

                if (isSuperSkill) {
                    if (trimType == "id")
                        skillController.superSkill = newSkill;
                    else {
                        oldValue = skillController.superSkill.GetSkillIdentifier(trimType);
                        skillController.superSkill.SetSkillIdentifier(trimType, Operator.Operate(op, oldValue, newValue));
                    }   

                    isSuccess = true;
                    continue;
                }
                
                if (normalSkillIndex >= 0) {
                    if (trimType == "id")
                        skillController.normalSkills[normalSkillIndex] = newSkill;
                    else {
                        oldValue = skillController.normalSkills[normalSkillIndex].GetSkillIdentifier(trimType);
                        skillController.normalSkills[normalSkillIndex].SetSkillIdentifier(trimType, Operator.Operate(op, oldValue, newValue));
                    }
                    
                    isSuccess = true;
                    continue;
                }
            }

            return isSuccess;
        }

        if (type.TryTrimStart("normalSkill", out var trimNormalSkill)) {
            TryGetShiftedSkills(battlePet, out var normalSkills, out var superSkill, normalSkillKey: "value");
            if (trimNormalSkill.TryTrimParentheses(out var skillIndexExpr)
                && int.TryParse(skillIndexExpr, out var skillIndex)) 
            {
                var newSkill = normalSkills.Get(skillIndex, normalSkills.FirstOrDefault());
                battlePet.skillController.normalSkills.Set(skillIndex, newSkill);
                return true;
            }
            battlePet.skillController.normalSkills = normalSkills.ToList();
            return true;
        }

        if (type == "superSkill") {
            TryGetShiftedSkills(battlePet, out var normalSkills, out var superSkill, superSkillKey: "value");
            battlePet.skillController.superSkill = superSkill;
            return true;
        }

        if (type == "specialChance") {
            lhsUnit.petSystem.specialChance = (int)Operator.Operate(op, lhsUnit.petSystem.specialChance, newValue);
            return true;
        }

        battlePet.SetPetIdentifier(type, Operator.Operate(op, oldValue, newValue));

        return true;
    }

    public static bool SetWeather(this Effect effect, BattleState state) {
        string weatherId = effect.abilityOptionDict.Get("weather", "0");

        if (!int.TryParse(weatherId, out int weather))
            return false;

        if (state.masterUnit.pet.buffController.IsBuffTypeBlocked(BuffType.Weather) || 
            state.clientUnit.pet.buffController.IsBuffTypeBlocked(BuffType.Weather))
            return false;

        state.weather = weather;
        return true;
    }

    public static bool SetPlayer(this Effect effect, BattleState state, 
        NpcController npc = null, NpcButtonHandler handler = null, 
        Dictionary<int, NpcController> npcList = null) {
        string action = effect.abilityOptionDict.Get("action", "none");
        string value = effect.abilityOptionDict.Get("param_count", "0");

        if (!int.TryParse(value, out var count))
            return false;

        // 對玩家的進行的效果，PVP不生效
        if ((state != null) && (state.settings.mode == BattleMode.PVP))
            return false;

        List<string> paramList = new List<string>();
        for (int i = 0; i < count; i++)
            paramList.Add(effect.abilityOptionDict.Get("param[" + i + "]").Replace("[ENDL]", "\n")
                .Replace("，", ",").Replace("＝", "=").Replace("＆", "&").Replace("｜", "|"));
        
        handler ??= new NpcButtonHandler() {
            actionType = action,
            param = paramList,
        };

        NpcHandler.GetNpcAction(npc, handler, npcList)?.Invoke();
        return true;
    }

    public static bool Poker(this Effect effect, BattleState state) {
        var who = effect.abilityOptionDict.Get("who", "me");
        var type = effect.abilityOptionDict.Get("type", "get");
        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        BattlePet battlePet = lhsUnit.pet;
        var buffController = battlePet.buffController;
        var cards = buffController.GetRangeBuff(x => (x.info.options.Get("group") == "poker") && (x.turn < 0));
        var count = cards.Count;

        bool GetCard() {
            if (cards.Count >= 52)
                return false;
                
            var color = 3200 + Random.Range(0, 4);
            var point = Random.Range(1, 14);
            while (!ListHelper.IsNullOrEmpty(buffController.GetRangeBuff(x => (x.id == color) && (x.value == point) && (x.turn < 0)))) {
                color = 3200 + Random.Range(0, 4);
                point = Random.Range(1, 14);
            }
            var buff = new Buff(color, -1, point);
            return buffController.AddBuff(buff, lhsUnit, state);
        }

        bool isSuccess = true;
        switch (type) {
            default:
                return false;
            case "get":
                return GetCard();
            case "refresh":
                isSuccess &= buffController.RemoveRangeBuff(x => (x.info.options.Get("group") == "poker") && (x.turn < 0), lhsUnit, state); 
                for (int i = 0; i < count; i++)
                    isSuccess &= GetCard();
                
                return isSuccess;
        }
    }
}

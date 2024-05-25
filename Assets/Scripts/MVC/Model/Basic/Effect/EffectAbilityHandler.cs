using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;

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
        return true;
    }

    public static bool Escape(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        state.phase = EffectTiming.OnBattleEnd;
        state.result.state = state.atkUnit.IsMyUnit() ? BattleResultState.MyEscape : BattleResultState.OpEscape;
        state.myUnit.isDone = state.opUnit.isDone = true;
        return true;
    }

    public static bool Capture(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        string prob = effect.abilityOptionDict.Get("prob", "0");
        float value;
        if (!float.TryParse(prob, out value))
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

        bool isSuccess = (value >= lhsUnit.RNG());
        lhsUnit.skill.options.Set("capture_result", isSuccess ? "true" : "false");
        
        if (isSuccess) {
            state.phase = EffectTiming.OnBattleEnd;
            state.result.state = BattleResultState.CaptureSuccess;
            state.myUnit.isDone = state.opUnit.isDone = true;
        }
        return true;
    }

    public static bool PetChange(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        string target = effect.abilityOptionDict.Get("target_index", "-1");
        int targetIndex = int.Parse(target);

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit petChangeUnit = state.GetUnitById(invokeUnit.id);

        bool isDead = petChangeUnit.pet.isDead;
        float anger = petChangeUnit.pet.anger;
        var inheritBuffs = petChangeUnit.pet.buffs.FindAll(x => x.info.inherit);

        // petChangeUnit.pet.buffController.RemoveRangeBuff(inheritBuffs, petChangeUnit, state);
        petChangeUnit.pet.buffController.RemoveRangeBuff(x => !x.info.keep, petChangeUnit, state);
        petChangeUnit.petSystem.cursor = targetIndex;
        petChangeUnit.pet.anger = Mathf.FloorToInt(anger * 0.8f);

        if (!isDead)
            petChangeUnit.pet.buffController.AddRangeBuff(inheritBuffs, petChangeUnit, state);

        if (petChangeUnit.id == state.myUnit.id)
            state.result.AddFightPetCursor(targetIndex);
        
        return true;
    }

    public static bool Heal(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "skill");
        string add = effect.abilityOptionDict.Get("add", "0");
        string set = effect.abilityOptionDict.Get("set","none");
        string max = effect.abilityOptionDict.Get("max","none");
        
        float heal = 0;
        if (state == null) {
            if (!float.TryParse(add, out heal))
                return false;

            Pet healPet = (Pet)effect.invokeUnit;
            healPet.currentStatus.hp = Mathf.Clamp(healPet.currentStatus.hp + heal, 0, healPet.normalStatus.hp);
            return true;
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        heal = Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit);
        int healAdd = (int)(heal * ((heal > 0) ? (lhsUnit.pet.battleStatus.rec / 100f) : 1));

        if (set != "none") {
            var setHp = (int)Parser.ParseEffectOperation(set, effect, lhsUnit, rhsUnit);
            if ((setHp < 0) && (lhsUnit.pet.buffController.GetBuff(99) != null))
                return false;
            
            lhsUnit.pet.hp = setHp;
            return true;
        }

        if (max != "none") {
            var maxHp = (int)Parser.ParseEffectOperation(max, effect, lhsUnit, rhsUnit);
            if ((maxHp < 0) && (lhsUnit.pet.buffController.GetBuff(99) != null))
                return false;

            lhsUnit.pet.maxHp = maxHp;
            return true;
        }

        // If already dead, no heal.
        if (lhsUnit.pet.isDead)
            return false;
        
        if ((type == "item") && (who == "me")) {
            lhsUnit.skillSystem.itemHeal += healAdd;
        } else if ((type == "skill") && (who == "me")) {
            lhsUnit.skillSystem.skillHeal += healAdd;;
        } else if (type == "buff") {
            lhsUnit.skillSystem.buffHeal += healAdd;
            healAdd = 0;
        }
        lhsUnit.pet.hp += healAdd;
        return true;
    }

    public static bool Rage(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string mult = effect.abilityOptionDict.Get("mult", "0/1");
        string add = effect.abilityOptionDict.Get("add", "0");
        string min = effect.abilityOptionDict.Get("min", "none");
        string max = effect.abilityOptionDict.Get("max", "none");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        var statusController = lhsUnit.pet.statusController;
        statusController.minAnger = (min == "none") ? statusController.minAnger : (int)Parser.ParseEffectOperation(min, effect, lhsUnit, rhsUnit);
        statusController.maxAnger = (max == "none") ? statusController.maxAnger : (int)Parser.ParseEffectOperation(max, effect, lhsUnit, rhsUnit);

        float anger = Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit);
        int angerAdd = (int)(anger * ((anger > 0) ? (lhsUnit.pet.battleStatus.angrec / 100f) : 1));
        lhsUnit.pet.anger += angerAdd;
        return true;
    }

    //! Note that currently block powerup/down should be together with clear powerup/down
    public static bool Powerup(this Effect effect, BattleState state) {
        Status status = new Status();
        string[] typeNames = Status.typeNames;

        string who = effect.abilityOptionDict.Get("who", "me");
        string random = effect.abilityOptionDict.Get("random", "false");

        bool isRandom;
        if (!bool.TryParse(random, out isRandom))
            return false;

        for (int type = 0; type < typeNames.Length - 1; type++) {
            int powerup = 0;
            string add = effect.abilityOptionDict.Get(typeNames[type], "0");
            if (!int.TryParse(add, out powerup))
                return false;
            status[type] = powerup;
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        BattlePet pet = lhsUnit.pet;
        var statusController = pet.statusController;
        var buffController = pet.buffController;

        if (isRandom) {
            string pdf = effect.abilityOptionDict.Get("random_pdf", "none");
            int count = status.Count(x => x != 0);

            if (pdf == "none") {
                // pass.
            } else if (pdf == "uniform") {
                int type = Random.Range(0, count);
                for (int i = 0, index = 0; i < typeNames.Length - 1; i++) {
                    if (status[i] == 0)
                        continue;
                    
                    status[i] = (index == type) ? status[i] : 0;
                    index++;
                }
            } else {
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

        if (buffController.GetBuff(45) != null)
            status = status.Select(x => x * (x > 0 ? 2 : 1));

        if (buffController.GetBuff(46) != null)
            status = status.Select(x => x * (x < 0 ? 2 : 1));

        pet.PowerUp(status, lhsUnit, state);

        return true;
    }

    //! Note that add hp wont work, since it should be heal.
    //! Note that add anger wont work, since it should be rage.
    public static bool AddStatus(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "5");
        string mult = effect.abilityOptionDict.Get("mult", "0/1");
        string add = effect.abilityOptionDict.Get("add", "0");

        int statusType;
        if (!int.TryParse(type, out statusType))
            return false;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        var pet = lhsUnit.pet;
        float statusMult = Parser.ParseEffectOperation(mult, effect, lhsUnit, rhsUnit);
        int statusAdd = (int)Parser.ParseEffectOperation(add, effect, lhsUnit, rhsUnit);

        statusAdd += Mathf.FloorToInt(pet.initStatus[statusType] * statusMult);
        pet.statusController.AddBattleStatus(statusType, statusAdd);

        return true;
    }

    public static bool BlockBuff(this Effect effect, BattleState state) {
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

        if (op == "+") {
            buffController.BlockBuff(idBlockList);
            buffController.BlockBuffWithType(typeBlockList);
        } else if (op == "-") {
            buffController.UnblockBuff(idBlockList);
            buffController.UnblockBuffWithType(typeBlockList);
        }
        return true;
    }

    public static bool AddBuff(this Effect effect, BattleState state) {
        string key = effect.abilityOptionDict.Get("key", string.Empty);
        string who = effect.abilityOptionDict.Get("who", "me");
        string id = effect.abilityOptionDict.Get("id", "0");
        string turn = effect.abilityOptionDict.Get("turn", "-1");
        string value = effect.abilityOptionDict.Get("value", "0");

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        int buffId = (int)Parser.ParseEffectOperation(id, effect, lhsUnit, rhsUnit);
        if (string.IsNullOrEmpty(key) && (Buff.GetBuffInfo(buffId) == null))
            return false;

        var buffController = lhsUnit.pet.buffController;
        int buffTurn = (int)Parser.ParseEffectOperation(turn, effect, lhsUnit, rhsUnit);
        int buffValue = (int)Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
        Buff newBuff = new Buff(buffId, buffTurn, buffValue);

        if (!string.IsNullOrEmpty(key)) {
            state.stateBuffs.RemoveAll(x => x.Key == key);
            if (newBuff != null)
                state.stateBuffs.Add(new KeyValuePair<string, Buff>(key, newBuff));
            
            return true;
        }

        buffController.AddBuff(newBuff, lhsUnit, state);
        return true;
    }

    public static bool RemoveBuff(this Effect effect, BattleState state) {
        string keyList = effect.abilityOptionDict.Get("key", string.Empty);
        string who = effect.abilityOptionDict.Get("who", "me");
        string idList = effect.abilityOptionDict.Get("id", "0");
        string typeList = effect.abilityOptionDict.Get("type", "none");

        string[] keyRange = keyList.Split('/');
        List<int> idRange = idList.ToIntList('/');
        string[] typeRange = typeList.Split('/');

        bool isKey = (!string.IsNullOrEmpty(keyList));
        bool isId = (idList != "0") && (idRange.Count != 0);
        bool isType = (typeList != "none");

        if (isKey) {
            state.stateBuffs.RemoveAll(x => keyRange.Contains(x.Key));
            return true;
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        var buffController = lhsUnit.pet.buffController;
        var statusController = lhsUnit.pet.statusController;

        if (isType) {
            foreach (var type in typeRange) {
                var buffType = type.ToBuffType();
                if (buffType == BuffType.TurnBased)
                    buffController.RemoveRangeBuff(x => (!x.id.IsWithin(-10_0000, 0)) && (x.turn > 0), lhsUnit, state);
                else 
                    buffController.RemoveRangeBuff(x => (!x.id.IsWithin(-10_0000, 0)) && (x.info.type == buffType), lhsUnit, state);
            }
            return true;
        }
        if (isId) {
            foreach (var id in idRange) {
                if (!new Buff(id).IsPower())
                    continue;

                int buffId = (id - 1) / 2;

                // id % 2 == 1 => want to clear powerup
                // powerup < 0 => current is powerdown
                // ^ is xor operator, which returns false when both true or both false
                if ((id % 2 == 1) ^ (statusController.powerup[buffId] < 0))
                    statusController.SetPowerUp(buffId, 0);
            }
            buffController.RemoveRangeBuff(x => idRange.Contains(x.id), lhsUnit, state);
            return true;
        }

        return false;
    }

    public static bool CopyBuff(this Effect effect, BattleState state) {
        string idList = effect.abilityOptionDict.Get("id", "0");
        string typeList = effect.abilityOptionDict.Get("type", "none");
        string source = effect.abilityOptionDict.Get("source", "op");
        string target = effect.abilityOptionDict.Get("target", "me");
        string transfer = effect.abilityOptionDict.Get("transfer", "false");
        string reverse = effect.abilityOptionDict.Get("reverse", "false");

        List<int> idRange = idList.ToIntList('/');
        string[] typeRange = typeList.Split('/');
        bool isId = (idList != "0") && (idRange.Count != 0);
        bool isType = (typeList != "none");
        if (!isId && !isType)
            return false;

        if (!bool.TryParse(transfer, out bool isTransfer) || !bool.TryParse(reverse, out bool isReverse))
            return false;
        
        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (source == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = (target == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        var lhsBuffController = lhsUnit.pet.buffController;
        var rhsBuffController = rhsUnit.pet.buffController;
        var copyType = typeList.ToBuffType();
        IEnumerable<Buff> buffs = null;

        if (isId) {
            buffs = lhsBuffController.GetRangeBuff(x => idRange.Contains(x.id));
            var powerup = lhsUnit.pet.statusController.powerup;
            var status = new Status();
            foreach (var buff in buffs) {
                if (!buff.IsPower()) {
                    rhsBuffController.AddBuff(new Buff(buff), rhsUnit, state);
                    continue;
                }
                
                int type = (buff.id - 1) / 2;
                status[type] = (isReverse ? -1 : 1) * powerup[type];

                if (isTransfer || isReverse) {
                    lhsUnit.pet.statusController.SetPowerUp(type, 0);
                }
            }
            rhsUnit.pet.PowerUp(status, rhsUnit, state);
        }
        if (isType) {
            buffs = lhsBuffController.GetRangeBuff(x => x.info.type == copyType);
            rhsBuffController.AddRangeBuff(buffs.Select(x => new Buff(x)), rhsUnit, state);
        }

        if (isTransfer) {
            lhsBuffController.RemoveRangeBuff(buffs, lhsUnit, state);
        }
        return true;
    }

    public static bool SetBuff(this Effect effect, BattleState state) {
        string key = effect.abilityOptionDict.Get("key", string.Empty);
        string who = effect.abilityOptionDict.Get("who", "me");
        string id = effect.abilityOptionDict.Get("id", "0");
        string type = effect.abilityOptionDict.Get("type", "value");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");

        int buffId;
        if (!int.TryParse(id, out buffId))
            return false;

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        var buff = string.IsNullOrEmpty(key) ? lhsUnit.pet.buffController.GetBuff(buffId) : state.stateBuffs.Find(x => x.Key == key).Value;
        if (buff == null)
            return false;

        float oldValue = buff.GetBuffIdentifier(type);
        float newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
        buff.SetBuffIdentifier(type, Operator.Operate(op, oldValue, newValue));

        if (buff.info.autoRemove && buff.value <= 0) {
            if (string.IsNullOrEmpty(key))
                lhsUnit.pet.buffController.RemoveBuff(buff, lhsUnit, state);
            else
                state.stateBuffs.RemoveAll(x => x.Value == buff);
        }
        return true;
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

        if (state.phase == EffectTiming.OnTurnEnd) {
            skillSystem.buffDamage = (int)Operator.Operate(op, skillSystem.buffDamage, damage);
        } else if (type == "skill") {
            skillSystem.skillDamage = (int)Operator.Operate(op, skillSystem.skillDamage, damage);
        } else if (type == "item") {
            skillSystem.itemDamage = (int)Operator.Operate(op, skillSystem.itemDamage, damage);
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
                "-4" => Skill.GetEscapeSkill(),
                "random" => Skill.GetRandomSkill(),
                _ => Skill.GetSkill(int.Parse(value)),
            });

            if (!isSetAnger)
                newSkill.anger = lhsUnit.skill.anger;

            lhsUnit.skillSystem.skill = newSkill;

            if (isResetParam)
                lhsUnit.skillSystem.PrepareDamageParam(state.atkUnit.pet, state.defUnit.pet);

            return true;
        }

        var skill = lhsUnit.skillSystem;
        float oldValue = Identifier.GetSkillIdentifier(type, skill);
        float newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);
        skill.SetSkillSystemIdentifier(type, Operator.Operate(op, oldValue, newValue));
        return true;
    }

    public static bool SetPet(this Effect effect, BattleState state) {
        string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "none");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");
        float oldValue, newValue;

        if (state == null) {
            Pet pet = (Pet)effect.invokeUnit;

            oldValue = pet.GetPetIdentifier(type);
            newValue = pet.TryGetPetIdentifier(value, out newValue) ? newValue : Identifier.GetNumIdentifier(value);

            // Evolve pet.
            if ((type == "id") && (op == "SET")) {
                var evolveId = (int)newValue;
                if (Pet.GetPetInfo(evolveId) == null)
                    return false;

                pet.MegaEvolve(evolveId);
                return true;
            }

            // Reset ev.
            if (type == "evReset") {
                pet.talent.ResetEV();
                return true;
            }

            // Set personality.
            if ((type == "personality") && (value == "-1")) {
                var petBagController = GameObject.FindObjectOfType<PetBagController>();
                if (petBagController == null)
                    return false;

                petBagController.SetPetPersonality();
                return true;
            }

            // Learn skill.
            if ((type == "skill") && (op == "+")) {
                if (!int.TryParse(value, out var skillId))
                    return false;

                var skill = Skill.GetSkill(skillId, false);
                if (skill == null)
                    return false;

                if (!pet.skills.LearnNewSkill(skill))
                    return false;
                    
                SaveSystem.SaveData();
                return true;
            }

            if (type == "buff") {
                if (!int.TryParse(value, out var buffId))
                    return false;
                
                if (op == "+")
                    pet.feature.afterwardBuffIds.Add(buffId);
                else
                    pet.feature.afterwardBuffIds.Remove(buffId);
                    
                SaveSystem.SaveData();
            }   

            pet.SetPetIdentifier(type, Operator.Operate(op, oldValue, newValue));
            return true;
        }

        Unit invokeUnit = (Unit)effect.invokeUnit;
        Unit lhsUnit = (who == "me") ? state.GetUnitById(invokeUnit.id) : state.GetRhsUnitById(invokeUnit.id);
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        BattlePet battlePet = lhsUnit.pet;

        oldValue = battlePet.GetPetIdentifier(type);
        newValue = Parser.ParseEffectOperation(value, effect, lhsUnit, rhsUnit);

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
                petSystem.petBag[index] = null;
            }

            // Prepare skill param.
            var normalSkillExpr = effect.abilityOptionDict.Get("normal_skill", "none");
            var superSkillExpr = effect.abilityOptionDict.Get("super_skill", "0");

            Skill[] normalSkills = null;
            Skill superSkill = null;

            // If normal_skill/super_skill is shift[COUNT], shift current skill ids with COUNT.
            // Else take it as an int list and get skill ids.
            if (normalSkillExpr.TryTrimStart("shift", out var normalTrim) &&
                normalTrim.TryTrimParentheses(out var normalShift) &&
                int.TryParse(normalShift, out var normalShiftCount)) {
                normalSkills = battlePet.normalSkill.Where(x => x != null).Select(x => Skill.GetSkill(x.id + normalShiftCount, false)).ToArray();
            } else {
                normalSkills = normalSkillExpr.ToIntList('/').Take(4).Select(id => Skill.GetSkill(id, false)).ToArray();
            }

            if (superSkillExpr.TryTrimStart("shift", out var superTrim) &&
                superTrim.TryTrimParentheses(out var superShift) &&
                int.TryParse(superShift, out var superShiftCount)) {
                superSkill = (battlePet.superSkill == null) ? null : (Skill.GetSkill(battlePet.superSkill.id + superShiftCount, false));
            } else {
                var superSkillId = (int)Parser.ParseEffectOperation(superSkillExpr, effect, lhsUnit, rhsUnit);
                superSkill = Skill.GetSkill(superSkillId, false);
            }

            // Switch Pet.
            Pet switchPet = Pet.GetExamplePet((int)newValue);
            switchPet.basic.personality = Personality.实干;
            switchPet.normalSkill = normalSkills;
            switchPet.superSkill = superSkill;

            var cursor = lhsUnit.petSystem.cursor;
            lhsUnit.petSystem.petBag[cursor] = BattlePet.GetBattlePet(switchPet);

            // Add feature and emblem buffs
            List<Buff> buffs = new List<Buff>();
            buffs.Add(Buff.GetFeatureBuff(lhsUnit.pet.info));
            buffs.Add(Buff.GetEmblemBuff(lhsUnit.pet.info));
            buffs.AddRange(lhsUnit.pet.initBuffs);

            lhsUnit.pet.buffController.AddRangeBuff(buffs, lhsUnit, state);

            return true;
        }

        // Switch skill.
        if (type.TryTrimStart("skill", out var trimType) &&
            trimType.TryTrimParentheses(out var trimSkillExpr)) {
            int trimSkillId = (int)Parser.ParseEffectOperation(trimSkillExpr, effect, lhsUnit, rhsUnit);
            bool isSuperSkill = ((lhsUnit.pet.superSkill?.id ?? 0) == trimSkillId);
            int normalSkillIndex = lhsUnit.pet.normalSkill.FindIndex(x => (x?.id ?? 0) == trimSkillId);

            Skill newSkill = value switch {
                "-1" => Skill.GetNoOpSkill(),
                "-4" => Skill.GetEscapeSkill(),
                "random" => Skill.GetRandomSkill(),
                _ => Skill.GetSkill((int)newValue, false),
            };
            newSkill = (newSkill == null) ? null : new Skill(newSkill);

            if (isSuperSkill) {
                lhsUnit.pet.superSkill = newSkill;
                return true;
            }
            
            if (normalSkillIndex >= 0) {
                var normalSkill = lhsUnit.pet.normalSkill.ToArray();
                normalSkill[normalSkillIndex] = newSkill;
                lhsUnit.pet.normalSkill = normalSkill;
                return true;
            }

            return false;
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

    public static bool SetPlayer(this Effect effect, BattleState state) {
        // string who = effect.abilityOptionDict.Get("who", "me");
        string type = effect.abilityOptionDict.Get("type", "none");
        string op = effect.abilityOptionDict.Get("op", "+");
        string value = effect.abilityOptionDict.Get("value", "0");
        // float oldValue, newValue;

        // 對玩家的進行的效果，PVP不生效
        if (state.settings.mode == BattleMode.PVP)
            return false;

        // Set item.
        if (type == "item") {
            var itemInfo = value.Split('/').Select(x => (int)Identifier.GetNumIdentifier(x)).ToList();
            Action<Item> itemFunc = op switch {
                "+" => Item.Add,
                "-" => (x) => Item.Remove(x.id, x.num),
                _ => null
            };
            string itemHint = op switch {
                "+" => "获得",
                "-" => "失去",
                _ => string.Empty,
            };
            if (itemFunc != null) {
                var item = new Item(itemInfo[0], itemInfo[1]);
                itemFunc.Invoke(item);
                ItemHintbox itemHintbox = Hintbox.OpenHintbox<ItemHintbox>();
                itemHintbox.SetTitle("提示");
                itemHintbox.SetContent(itemHint + "了 " + item.num + " 个 " + item.name, 16, FontOption.Arial);
                itemHintbox.SetOptionNum(1);
                itemHintbox.SetIcon(item.icon);
            }
            return true;
        }
        return true;
    }
}

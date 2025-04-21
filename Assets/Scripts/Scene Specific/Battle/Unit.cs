using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;

public class Unit
{
    public int id { get; protected set; }
    public int random;
    public int parallelCount;
    public UnitPetSystem petSystem;
    public List<UnitSkillSystem> parallelSkillSystems = new List<UnitSkillSystem>();
    public UnitSkillSystem skillSystem {
        get => (parallelCount <= 1) ? parallelSkillSystems.FirstOrDefault() : parallelSkillSystems[petSystem.cursor];
        set => parallelSkillSystems.Update(skillSystem, value);
    }
    public UnitHudSystem hudSystem;

    public BattlePet pet => petSystem.pet;
    public Skill skill
    {
        get => skillSystem.skill;
        set => SetSkill(value);
    }
    public List<Buff> unitBuffs = new List<Buff>();

    /* Turn */
    public bool isReady => IsReady();
    public bool isDone = false;

    public Unit(BattlePet[] petBag, int idNum, BattleSettings settings)
    {
        id = idNum;
        random = Random.Range(0, 100);
        parallelCount = settings.parallelCount;
        petSystem = new UnitPetSystem(petBag);
        parallelSkillSystems = Enumerable.Range(0, (parallelCount <= 1) ? 1 : petBag.Length).Select(x => new UnitSkillSystem()).ToList();
        // skillSystem = new UnitSkillSystem();
        hudSystem = new UnitHudSystem();
        unitBuffs = new List<Buff>();
    }

    public Unit(Unit rhs)
    {
        id = rhs.id;
        random = rhs.random;
        parallelCount = rhs.parallelCount;
        isDone = rhs.isDone;
        petSystem = new UnitPetSystem(rhs.petSystem);
        parallelSkillSystems = rhs.parallelSkillSystems.Select(x => new UnitSkillSystem(x)).ToList();
        // skillSystem = new UnitSkillSystem(rhs.skillSystem);
        hudSystem = new UnitHudSystem(rhs.hudSystem);
        unitBuffs = rhs.unitBuffs.Select(x => new Buff(x)).ToList();
    }

    public virtual void OnTurnStart(BattleState state)
    {
        isDone = false;
        random = Random.Range(0, 100);
        petSystem.OnTurnStart(this, state);
        parallelSkillSystems.ForEach(x => x.OnTurnStart());
        hudSystem.OnTurnStart(this.pet);
        for (int i = 0; i < unitBuffs.Count; i++) {
            if (unitBuffs[i].turn > 0)
                unitBuffs[i].turn--;
        }
    }

    public bool IsReady()
    {
        if (parallelCount <= 1)
            return (skill != null) && skill.IsSelectReady();

        if (parallelSkillSystems.Exists(x => (x?.skill != null) && (x.skill.type == SkillType.逃跑)))
            return true;

        for (int i = 0; i < parallelSkillSystems.Count; i++) {
            var pet = petSystem.petBag[i];
            if ((pet == null) || (pet.isDead))
                continue;
            
            var skill = parallelSkillSystems[i].skill;
            if ((skill == null) || (!skill.IsSelectReady()))
                return false;
        }
        return true;
    }

    public bool IsMasterUnit()
    {
        return id == 1;
    }

    public bool IsMyUnit()
    {
        return PhotonNetwork.IsConnected ? (PhotonNetwork.IsMasterClient == IsMasterUnit()) : IsMasterUnit();
    }

    public float RNG()
    {
        return random = Random.Range(0, 100);
    }

    public void SetSkill(Skill _skill)
    {
        skillSystem.skill = (_skill == null) ? null : new Skill(_skill);
    }

    public bool AddBuff(Buff newBuff) 
    {
        if (newBuff == null)
            return false;

        var oldBuff = unitBuffs.Find(x => x.id == newBuff.id);
        if (oldBuff == null) {
            unitBuffs.Add(newBuff);
            return true;
        }

        switch (newBuff.info.copyHandleType) {
            default:
            case CopyHandleType.New:
                unitBuffs.Add(newBuff);
                return true;
            case CopyHandleType.Block:
                return false;
            case CopyHandleType.Replace:
                int oldBuffTurn = (oldBuff.turn == -1) ? int.MaxValue : oldBuff.turn;
                int newBuffTurn = (newBuff.turn == -1) ? int.MaxValue : newBuff.turn;
                if (oldBuffTurn <= newBuffTurn) {
                    unitBuffs.Remove(oldBuff);
                    unitBuffs.Add(newBuff);
                    return true;
                }
                return false;
            case CopyHandleType.Stack:
                if (oldBuff.value < oldBuff.info.maxValue) {
                    oldBuff.value += newBuff.value;
                    return true;
                }
                return false;
            case CopyHandleType.Max:
                if (newBuff.value > oldBuff.value) {
                    unitBuffs.Remove(oldBuff);
                    unitBuffs.Add(newBuff);
                    return true;
                }
                return false;
            case CopyHandleType.Min:
                if (newBuff.value < oldBuff.value) {
                    unitBuffs.Remove(oldBuff);
                    unitBuffs.Add(newBuff);
                    return true;
                }
                return false;
        }
    }

    public bool RemoveBuff(Predicate<Buff> predicate)
    {
        return unitBuffs.RemoveAll(predicate) > 0;
    }

    public void OnChainStart() 
    {
        skillSystem.OnChainStart();
    }

    public bool CalculateAccuracy(Unit rhs)
    {
        return skillSystem.CalculateAccuracy(pet, rhs.pet);
    }

    public void PrepareDamageParam(Unit rhs)
    {
        skillSystem.PrepareDamageParam(pet, rhs.pet);
    }

    public int CalculateDamage(Unit rhs)
    {
        return skillSystem.CalculateDamage(pet, rhs.pet);
    }
}
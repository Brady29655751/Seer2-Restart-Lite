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
    }

    public virtual void OnTurnStart(BattleState state)
    {
        isDone = false;
        random = Random.Range(0, 100);
        petSystem.OnTurnStart(this, state);
        parallelSkillSystems.ForEach(x => x.OnTurnStart());
        // skillSystem.OnTurnStart();
        hudSystem.OnTurnStart(this.pet);
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
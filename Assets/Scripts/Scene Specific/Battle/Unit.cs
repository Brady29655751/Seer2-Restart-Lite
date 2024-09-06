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
    public UnitPetSystem petSystem;
    public UnitSkillSystem skillSystem;
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

    public Unit(BattlePet[] petBag, int idNum)
    {
        id = idNum;
        random = Random.Range(0, 100);
        petSystem = new UnitPetSystem(petBag);
        skillSystem = new UnitSkillSystem();
        hudSystem = new UnitHudSystem();
    }

    public Unit(Unit rhs)
    {
        id = rhs.id;
        random = rhs.random;
        isDone = rhs.isDone;
        petSystem = new UnitPetSystem(rhs.petSystem);
        skillSystem = new UnitSkillSystem(rhs.skillSystem);
        hudSystem = new UnitHudSystem(rhs.hudSystem);
    }

    public virtual void OnTurnStart(BattleState state)
    {
        isDone = false;
        random = Random.Range(0, 100);
        petSystem.OnTurnStart(this, state);
        skillSystem.OnTurnStart();
        hudSystem.OnTurnStart(this.pet);
    }

    public bool IsReady()
    {
        return (skill != null);
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
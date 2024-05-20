using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class BattlePet : Pet
{
    public PetBattleStatusController statusController;
    public PetBattleBuffController buffController;
    public PetBattleSkillController skillController;

    /* Properties */
    public int battleElementId => (int)battleElement;
    public Element battleElement => buffController.element;
    public bool isDead => (battleStatus.hp <= 0);
    public bool isMovable => buffController.isMovable;

    public Status initStatus => statusController.initStatus;
    public BattleStatus battleStatus => statusController.battleStatus;

    public List<Buff> buffs => buffController.buffs;

    public int hp {
        get => statusController.hp;
        set => statusController.hp = value;
    }
    public int maxHp {
        get => statusController.maxHp;
        set => statusController.maxHp = value;
    }

    public int anger {
        get => statusController.anger;
        set => statusController.anger = value;
    }
    public int minAnger => statusController.minAnger;
    public int maxAnger => statusController.maxAnger;
    

    public BattlePet(Pet _pet) : base(_pet) {
        if (_pet == null)
            return;

        statusController = new PetBattleStatusController(normalStatus, currentStatus.hp);
        buffController = new PetBattleBuffController(element, _pet.initBuffs);
        skillController = new PetBattleSkillController(normalSkill.ToList(), superSkill);
    }

    public BattlePet(BossInfo info) : base(info.petId, info.level) {
        feature.afterwardBuffIds = info.initBuffs;
        Status basicStatus = (info.status == null) ? normalStatus : info.status.GetBasicStatus().Select((x, i) => (x == 0) ? normalStatus[i] : x);
        Status hiddenStatus = (info.status == null) ? new Status(0, 0, 0, 0, 100, 100) : info.status.GetHiddenStatus();
        BattleStatus status = new BattleStatus(basicStatus, hiddenStatus);

        statusController = new PetBattleStatusController(status);
        buffController = new PetBattleBuffController(element, initBuffs);
        skillController = new PetBattleSkillController(info.loopSkills, info.headerSkills, info.superSkill);

        normalSkill = info.normalSkills;
        superSkill = info.superSkill;
    }

    public BattlePet(BattlePet rhs) : base(rhs) {
        if (rhs == null)
            return;
        
        statusController = new PetBattleStatusController(rhs.statusController);
        buffController = new PetBattleBuffController(rhs.buffController);
        skillController = new PetBattleSkillController(rhs.skillController);
    }

    public static BattlePet GetBattlePet(Pet pet) {
        return (pet == null) ? null : new BattlePet(pet);
    }

    public static BattlePet GetBattlePet(BossInfo info) {
        return (info == null) ? null : new BattlePet(info);
    }

    /// <summary>
    /// Get battle pet for PVP mode.
    /// This automatically heal the pet to prevent player forgot it.
    /// </summary>
    public static BattlePet GetBattlePet(int id, int personality, float[] ev, int[] normalSkill, int superSkill) {
        Pet pet = Pet.GetExamplePet(id);
        if (pet == null)
            return null;

        pet.basic.personality = (Personality)personality;
        pet.talent.ev = new Status(ev);
        pet.skills.normalSkillId = normalSkill;
        pet.skills.superSkillId = superSkill;
        pet.currentStatus = new Status(pet.normalStatus);
        return new BattlePet(pet);
    }

    public static BattlePet[] GetBattlePetBag(Hashtable hash, int petCount) {
        var id = (int[])hash["pet"];
        var personality = (int[])hash["char"];
        var ev = (float[][])hash["ev"];
        var normalSkill = (int[][])hash["skill"];
        var superSkill = (int[])hash["super"];

        return Enumerable.Range(0, petCount).Select(i => BattlePet.GetBattlePet(
            id[i], personality[i], ev[i], normalSkill[i], superSkill[i]
        )).ToArray();
    }

    public static KeyValuePair<int, int> GetSkillTypeStatus(Skill skill, BattlePet lhs, BattlePet rhs) {
        Status battleStatus = new Status(lhs.battleStatus.atk, lhs.battleStatus.mat, 
            rhs.battleStatus.def, rhs.battleStatus.mdf, 0, 0);
        battleStatus.spd = (battleStatus.atk + battleStatus.mat) / 2;
        battleStatus.hp = (battleStatus.def + battleStatus.mdf) / 2;

        Status initStatus = new Status(lhs.battleStatus.atk, lhs.battleStatus.mat,
            rhs.initStatus.def, rhs.initStatus.mdf, 0, 0);
        initStatus.spd = (initStatus.atk + initStatus.mat) / 2;
        initStatus.hp = (initStatus.def + initStatus.mdf) / 2;

        var status = skill.ignorePowerup ? initStatus : battleStatus;

        if (skill.type == SkillType.物理) {
            return new KeyValuePair<int, int>((int)status.atk, (int)status.def);
        }
        if (skill.type == SkillType.特殊) {
            return new KeyValuePair<int, int>((int)status.mat, (int)status.mdf);
        }
        if (skill.type == SkillType.必杀) {
            if (status.atk > status.mat) {
                return new KeyValuePair<int, int>((int)status.atk, (int)status.def);
            }
            if (status.atk < status.mat) {
                return new KeyValuePair<int, int>((int)status.mat, (int)status.mdf);
            }
            return new KeyValuePair<int, int>((int)status.spd, (int)status.hp);
        }
        return new KeyValuePair<int, int>(0, 1);
    }

    public override float GetPetIdentifier(string id) {
        return id switch {
            "element" => (int)buffController.element,
            _ => base.GetPetIdentifier(id),
        };
    }

    public override bool TryGetPetIdentifier(string id, out float value) {
        value = GetPetIdentifier(id);
        return value != float.MinValue;
    }

    public override void SetPetIdentifier(string id, float value) {
        switch (id) {
            default:
                base.SetPetIdentifier(id, value);
                return;
            case "element":
                buffController.SetElement((Element)value);
                return;
        }
    }

    public virtual Skill GetDefaultSkill() {
        return skillController.GetDefaultSkill(anger);
    }

    public virtual void PowerUp(Status status, Unit lhsUnit, BattleState state) {

        buffController.RemoveRangeBuff(x => x.IsPower(), lhsUnit, state);

        for (int type = 0; type < Status.typeNames.Length - 1; type++) {
            var powerup = (int)status[type];
            var id = Buff.GetPowerUpBuffId(type, powerup);

            if (!buffController.IsBuffBlocked(id)) {
                statusController.AddPowerUp(type, powerup);
            }

            powerup = statusController.GetPowerUp(type);
            id = Buff.GetPowerUpBuffId(type, powerup);     
            if (id == 0)
                continue;

            var buff = new Buff(id, -1, Mathf.Abs(powerup));
            buffController.AddBuff(buff, lhsUnit, state);
        }
    }

    public virtual void OnTurnStart(Unit thisUnit, BattleState state) {
        buffController.OnTurnStart(thisUnit, state);
    }

}

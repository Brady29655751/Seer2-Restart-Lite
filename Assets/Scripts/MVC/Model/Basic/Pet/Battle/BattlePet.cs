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
    public bool isDead => (battleStatus.hp <= 0);
    public bool isMovable => buffController.isMovable;

    public Status initStatus => statusController.initStatus;
    public BattleStatus battleStatus => statusController.battleStatus;

    public List<Buff> buffs => buffController.buffs;

    public int hp {
        get => statusController.hp;
        set => statusController.hp = value;
    }
    public int maxHp => statusController.maxHp;

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
        buffController = new PetBattleBuffController(element, null);
        skillController = new PetBattleSkillController(normalSkill.ToList(), superSkill);
    }

    public BattlePet(BossInfo info) : base(info.petId, info.level) {
        List<Buff> initBuffs = info.initBuffs;
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
        Status status = new Status(lhs.battleStatus.atk, lhs.battleStatus.mat, 
            rhs.battleStatus.def, rhs.battleStatus.mdf, 0, 0);
        status.spd = (status.atk + status.mat) / 2;
        status.hp = (status.def + status.mdf) / 2;

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

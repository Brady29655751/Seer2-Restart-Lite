using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class BattlePet : Pet
{
    public int stayTurn = 0, chain = 0;
    public PetBattleStatusController statusController;
    public PetBattleBuffController buffController;
    public PetBattleSkillController skillController;

    /* Properties */
    public int battleElementId => (int)battleElement;
    public Element battleElement => buffController.element;

    public int subBattleElementId => (int)subBattleElement;
    public Element subBattleElement => buffController.subElement;

    public bool isDead => (battleStatus.hp <= 0);
    public bool isMovable => buffController.isMovable;
    public bool isToken => buffController.GetBuff(Buff.BUFFID_TOKEN) != null;

    public Status initStatus => statusController.initStatus;
    public BattleStatus battleStatus => statusController.battleStatus;

    public List<Buff> buffs => buffController.buffs;

    public int hp
    {
        get => statusController.hp;
        set => statusController.hp = value;
    }
    public int maxHp
    {
        get => statusController.maxHp;
        set => statusController.maxHp = value;
    }

    public int anger
    {
        get => statusController.anger;
        set => statusController.anger = value;
    }
    public int minAnger => statusController.minAnger;
    public int maxAnger => statusController.maxAnger;


    public BattlePet(Pet _pet) : base(_pet)
    {
        if (_pet == null)
            return;

        stayTurn = 0;
        chain = 0;
        statusController = new PetBattleStatusController(normalStatus, currentStatus.hp);
        buffController = new PetBattleBuffController(element, subElement, null);
        skillController = new PetBattleSkillController(normalSkill.ToList(), superSkill);
    }

    public BattlePet(BossInfo bossInfo) : base(bossInfo.petId, bossInfo.level, bossInfo.hasEmblem)
    {
        Status basicStatus = (bossInfo.status == null) ? normalStatus : bossInfo.status.GetBasicStatus().Select((x, i) => (x == 0) ? normalStatus[i] : x);
        Status hiddenStatus = (bossInfo.status == null) ? new Status(0, 0, 0, 0, 100, 100) : bossInfo.status.GetHiddenStatus();
        BattleStatus status = new BattleStatus(basicStatus, hiddenStatus);

        stayTurn = 0;
        chain = 0;

        statusController = new PetBattleStatusController(status);
        buffController = new PetBattleBuffController(element, subElement, bossInfo.initBuffs);
        skillController = new PetBattleSkillController(bossInfo.loopSkills, bossInfo.headerSkills, bossInfo.superSkill, bossInfo.normalSkills.ToList());

        normalSkill = bossInfo.normalSkills;
        superSkill = bossInfo.superSkill;
    }

    public BattlePet(BattlePet rhs) : base(rhs)
    {
        if (rhs == null)
            return;

        stayTurn = rhs.stayTurn;
        chain = rhs.chain;

        statusController = new PetBattleStatusController(rhs.statusController);
        buffController = new PetBattleBuffController(rhs.buffController);
        skillController = new PetBattleSkillController(rhs.skillController);
    }

    public static BattlePet GetBattlePet(Pet pet)
    {
        return (pet == null) ? null : new BattlePet(pet);
    }

    public static BattlePet GetBattlePet(BossInfo info)
    {
        return (info == null) ? null : new BattlePet(info);
    }

    /// <summary>
    /// Get battle pet for PVP mode.
    /// This automatically heal the pet to prevent player forgot it.
    /// </summary>
    public static BattlePet GetBattlePet(int id, int personality, int feature, int emblem, int[] buff, int iv, float[] ev, int[] normalSkill, int superSkill)
    {
        Pet pet = Pet.GetExamplePet(id, iv: iv);
        if (pet == null)
            return null;

        pet.basic.personality = (Personality)personality;
        pet.feature.featureId = feature;
        pet.feature.emblemId = emblem;
        pet.feature.afterwardBuffIds = buff.ToList();
        pet.talent.ev = new Status(ev);
        pet.skills.normalSkillId = normalSkill;
        pet.skills.superSkillId = superSkill;
        pet.currentStatus = new Status(pet.normalStatus);
        return new BattlePet(pet);
    }

    public static BattlePet[] GetBattlePetBag(Hashtable hash, int petCount, int iv)
    {
        var id = (int[])hash["pet"];
        var personality = (int[])hash["char"];
        var feature = (int[])hash["feature"];
        var emblem = (int[])hash["emblem"];
        var buff = (int[][])hash["buff"];
        var ev = (float[][])hash["ev"];
        var normalSkill = (int[][])hash["skill"];
        var superSkill = (int[])hash["super"];

        return Enumerable.Range(0, petCount).Select(i => (i >= id.Length) ? null :
            BattlePet.GetBattlePet(id[i], personality[i], feature[i], emblem[i], buff[i], iv, ev[i], normalSkill[i], superSkill[i]
        )).ToArray();
    }

    public static KeyValuePair<int, int> GetSkillTypeStatus(Skill skill, BattlePet lhs, BattlePet rhs)
    {
        var lhsStatus = lhs.statusController.GetCurrentStatus(skill.powerup, skill.powerdown);
        var rhsStatus = rhs.statusController.GetCurrentStatus(skill.powerup, skill.powerdown);

        Status status = new Status(lhsStatus.atk, lhsStatus.mat, rhsStatus.def, rhsStatus.mdf, 0, 0);
        status.spd = (status.atk + status.mat) / 2;
        status.hp = (status.def + status.mdf) / 2;

        if (skill.type == SkillType.物理)
        {
            return new KeyValuePair<int, int>((int)status.atk, (int)status.def);
        }
        if (skill.type == SkillType.特殊)
        {
            return new KeyValuePair<int, int>((int)status.mat, (int)status.mdf);
        }
        if ((skill.type == SkillType.必杀) || (skill.type == SkillType.连携))
        {
            if (status.atk > status.mat)
            {
                return new KeyValuePair<int, int>((int)status.atk, (int)status.def);
            }
            if (status.atk < status.mat)
            {
                return new KeyValuePair<int, int>((int)status.mat, (int)status.mdf);
            }
            return new KeyValuePair<int, int>((int)status.spd, (int)status.hp);
        }
        return new KeyValuePair<int, int>(0, 1);
    }

    public override float GetPetIdentifier(string id)
    {
        if (id.TryTrimStart("skill", out var trimSkill))
        {

            if (trimSkill.TryTrimParentheses(out var skillIdExpr) && int.TryParse(skillIdExpr, out var skillId))
            {
                trimSkill = trimSkill.TrimStart("[" + skillIdExpr + "]").TrimStart('.');
                trimSkill = string.IsNullOrEmpty(trimSkill) ? "id" : trimSkill;
                trimSkill = $"(id:{skillId}).{trimSkill}";
            }

            var filterList = trimSkill.TrimParenthesesLoop('(', ')');
            var filter = Parser.ParseConditionFilter<Skill>(trimSkill, (expr, skill) => skill?.GetSkillIdentifier(expr) ?? Identifier.GetNumIdentifier(expr));
            var allSkills = skillController.allSkills.Where(x => x != null).Where(filter).ToList();

            trimSkill = trimSkill.TrimStart("(" + filterList.ConcatToString(")(") + ")").TrimStart('.');
            var split = trimSkill.Split('.');
            if (split.Length <= 1)
                return split[0] switch
                {
                    "count" => allSkills.Count,
                    _ => allSkills.FirstOrDefault()?.GetSkillIdentifier(split[0]) ?? float.MinValue,
                };

            return split[1] switch
            {
                "sum" => allSkills.Sum(x => x.GetSkillIdentifier(split[0])),
                "max" => allSkills.Max(x => x.GetSkillIdentifier(split[0])),
                "min" => allSkills.Min(x => x.GetSkillIdentifier(split[0])),
                _ => allSkills.FirstOrDefault()?.GetSkillIdentifier(split[0]) ?? float.MinValue,
            };
        }

        if (id.TryTrimStart("normalSkill", out var trimNormalSkill) &&
            trimNormalSkill.TryTrimParentheses(out var skillIndexExpr) &&
            int.TryParse(skillIndexExpr, out var skillIndex))
        {
            var trimId = trimNormalSkill.TrimStart($"[{skillIndexExpr}]").TrimStart('.');
            if (string.IsNullOrEmpty(trimId))
                trimId = "id";

            var allSkills = ListHelper.IsNullOrEmpty(skillController?.loopSkills) ? skillController.normalSkills : skillController.loopSkills;
            return allSkills?.Get(skillIndex)?.GetSkillIdentifier(trimId) ?? 0;
        }

        if (id.TryTrimStart("superSkill", out var trimSuperSkill))
        {
            var trimId = trimSuperSkill.TrimStart('.');
            if (string.IsNullOrEmpty(trimId))
                trimId = "id";

            return skillController.superSkill?.GetSkillIdentifier(trimId) ?? 0;
        }

        return id switch
        {
            "stayTurn" => stayTurn,
            "chain" => chain,
            "element" => (int)buffController.element,
            "subElement" => (int)buffController.subElement,
            "movable" => isMovable ? 1 : 0,
            "dead" => isDead ? 1 : 0,
            "token" => isToken ? 1 : 0,
            _ => base.GetPetIdentifier(id),
        };
    }

    public override bool TryGetPetIdentifier(string id, out float value)
    {
        value = GetPetIdentifier(id);
        return value != float.MinValue;
    }

    public override void SetPetIdentifier(string id, float value)
    {
        switch (id)
        {
            default:
                base.SetPetIdentifier(id, value);
                return;
            case "element":
                buffController.SetElement((Element)value);
                return;
            case "subElement":
                buffController.SetSubElement((Element)value);
                return;
            case "stayTurn":
                stayTurn = (int)value;
                return;
            case "level":
            case "maxLevel":
            case "iv":
                var oldStatus = normalStatus;
                if (id == "level")
                    exp.level = (int)value;
                else
                    base.SetPetIdentifier(id, value);

                var addStatus = normalStatus - oldStatus;
                maxHp += (int)addStatus.hp;
                statusController.AddInitStatus(new Status(addStatus) { hp = 0 });
                normalSkill = skillController.normalSkills?.ToArray();
                superSkill = skillController.superSkill;
                return;
        }
    }

    public virtual Skill GetDefaultSkill()
    {
        var petAnger = (buffController.GetBuff(61) == null) ? anger : int.MaxValue;
        return skillController.GetDefaultSkill(petAnger);
    }

    public virtual void PowerUp(Status status, Unit lhsUnit, BattleState state)
    {

        buffController.RemoveRangeBuff(x => x.IsPower(), lhsUnit, state);

        for (int type = 0; type < Status.typeNames.Length; type++)
        {
            var powerup = (int)status[type];
            var id = Buff.GetPowerUpBuffId(type, powerup);

            if (!buffController.IsBuffIdBlocked(id))
            {
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

    public virtual void OnTurnStart(Unit thisUnit, BattleState state)
    {
        chain = 0;

        if (isToken)
            return;

        stayTurn += 1;
        buffController.OnTurnStart(thisUnit, state);
    }

    public bool IsSkillCostEnough(int skillId, BattleRule rule)
    {
        var skill = skillController.FindSkill(skillId);
        if (skill == null)
            return false;

        switch (rule)
        {
            default:
                return true;

            case BattleRule.Anger:
                return (statusController.anger >= skill.anger) || (buffController.GetBuff(61) != null);

            case BattleRule.PP:
                return skill.PP > 0;
        }
    }
}

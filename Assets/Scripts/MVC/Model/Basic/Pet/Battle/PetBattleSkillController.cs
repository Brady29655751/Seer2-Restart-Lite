using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBattleSkillController
{
    public int cursor = 0;
    public List<Skill> headerSkills;
    public List<Skill> loopSkills;
    public List<Skill> normalSkills;
    public Skill superSkill;
    public List<Skill> allSkills => (headerSkills ?? new List<Skill>())
        .Concat(loopSkills ?? new List<Skill>())
        .Concat(normalSkills ?? new List<Skill>())
        .Concat(superSkill?.SingleToList() ?? new List<Skill>()).ToList();

    public PetBattleSkillController(List<Skill> normalSkills, Skill superSkill = null)
    {
        this.normalSkills = normalSkills?.Select(x => (x == null) ? null : new Skill(x, false)).ToList();
        this.superSkill = (superSkill == null) ? null : new Skill(superSkill, false);
    }

    public PetBattleSkillController(List<Skill> loopSkills, List<Skill> headerSkills = null, Skill superSkill = null, List<Skill> normalSkills = null)
    {
        this.headerSkills = headerSkills?.Select(x => (x == null) ? null : new Skill(x, false)).ToList();
        this.loopSkills = loopSkills?.Select(x => (x == null) ? null : new Skill(x, false)).ToList();
        this.superSkill = (superSkill == null) ? null : new Skill(superSkill, false);
        this.normalSkills = normalSkills?.Select(x => (x == null) ? null : new Skill(x, false)).ToList();
    }

    public PetBattleSkillController(PetBattleSkillController rhs)
    {
        cursor = rhs.cursor;
        headerSkills = rhs.headerSkills?.Select(x => new Skill(x)).ToList();
        loopSkills = rhs.loopSkills?.Select(x => new Skill(x)).ToList();
        normalSkills = rhs.normalSkills?.Select(x => (x == null) ? null : new Skill(x)).ToList();
        superSkill = (rhs.superSkill == null) ? null : new Skill(rhs.superSkill);
    }

    public Skill GetDefaultSkill(int anger)
    {
        Skill defaultSkill = Skill.GetNoOpSkill();

        if ((superSkill != null) && (superSkill.anger <= anger) && (!superSkill.isSelect))
            return superSkill;

        if ((loopSkills == null) || (loopSkills.Count == 0))
            return GetAvailableSkills(anger, false).First();

        if ((headerSkills != null) && (headerSkills.Count > 0))
        {
            defaultSkill = headerSkills[cursor++];
            if (cursor >= headerSkills.Count)
            {
                headerSkills = null;
                cursor = 0;
            }
            return defaultSkill;
        }

        defaultSkill = loopSkills[cursor++];
        cursor %= loopSkills.Count;
        return defaultSkill;
    }

    public List<Skill> GetAvailableSkills(int anger, bool withSuper = true)
    {
        var availableSkills = (((normalSkills?.Count ?? 0) > 0) ? normalSkills : loopSkills)
            .Where(x => (x != null) && (x.anger <= anger) && (!x.isSelect)).ToList();

        if (withSuper && (superSkill != null) && (superSkill.anger <= anger) && (!superSkill.isSelect))
            availableSkills.Add(superSkill);

        if (availableSkills.Count == 0)
            availableSkills.Add(Skill.GetNoOpSkill());

        return availableSkills;
    }

    public Skill FindSkill(int id)
    {
        var headerIndex = headerSkills?.FindIndex(x => (x?.id ?? 0) == id) ?? -1;
        var loopIndex = loopSkills?.FindIndex(x => (x?.id ?? 0) == id) ?? -1;
        var normalIndex = normalSkills?.FindIndex(x => (x?.id ?? 0) == id) ?? -1;
        var isSuper = (superSkill != null) && (superSkill?.id == id);

        //if (isSuper)
        //    return superSkill;
        //
        //if (normalIndex >= 0)
        //    return normalSkills[normalIndex];
        //
        //if (headerIndex >= 0)
        //    return headerSkills[headerIndex];
        //
        //if (loopIndex >= 0)
        //    return loopSkills[loopIndex];

        //return null;

        return allSkills.Find(x => (x?.id ?? 0) == id);
    }
}

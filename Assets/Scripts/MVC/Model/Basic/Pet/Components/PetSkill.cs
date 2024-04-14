using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSkill
{   
    public PetSkillInfo info => Database.instance.GetPetInfo(id)?.skills;

    [XmlAttribute] public int id;
    public int[] ownSkillId = new int[0];   // 已習得的技能
    public int[] normalSkillId = new int[4];    // 使用的一般技能
    public int superSkillId;    // 使用的必殺技

    #region properties
    
    [XmlIgnore] public List<Skill> skillList => info.skillList;     // 可習得的技能

    /* Pet learned skills */
    [XmlIgnore] public List<Skill> ownSkill {
        get => ownSkillId.Select(x => Skill.GetSkill(x, false)).ToList();
        set => ownSkillId = (value == null) ? new int[1] : value.Select(x => x.id).ToArray();
    }
    
    /* Pet current skills */
    [XmlIgnore] public Skill[] normalSkill { 
        get => GetNormalSkill(); 
        set => normalSkillId = (value == null) ? new int[4] : value.Take(4).Select(x => (x == null) ? 0 : x.id).ToArray();
    }
    public LearnSkillInfo[] normalSkillInfo => GetLearnSkillInfos(normalSkillId);

    [XmlIgnore] public Skill superSkill {
        get => Skill.GetSkill(superSkillId, false);
        set => superSkillId = (value == null) ? 0 : value.id;
    }
    public LearnSkillInfo superSkillInfo => GetLearnSkillInfos(superSkillId);

    /* Pet backup skills */
    public Skill[] backupNormalSkill => ownSkill.Where(x => (x.type != SkillType.必杀) && 
        normalSkill.All(y => (y != null) && (x.id != y.id))).ToArray();
    
    public Skill backupSuperSkill => ownSkill.Where(x => x.type == SkillType.必杀).Where(
        x => (superSkill != null) && (x.id != superSkill.id)).FirstOrDefault();


    /* Pet secret skills */
    public int[] secretSkillId => secretSkill.Select(x => x.id).ToArray();
    public Skill[] secretSkill => info.learnInfoList.Where(x => (x.secretType > SecretType.GreaterThanLevel) && (x.secretType <= SecretType.SpecialPet)).Select(x => x.skill).ToArray();
    public LearnSkillInfo[] secretSkillInfo => GetLearnSkillInfos(secretSkillId);


    #endregion

    public PetSkill() {}

    public PetSkill(int _id, int _level, Skill[] defaultNormalSkill = null) {
        id = _id;
        CheckNewSkill(_level);
        if (defaultNormalSkill == null)
            return;

        defaultNormalSkill = defaultNormalSkill.Where(x => x != null).ToArray();
        for (int i = 0; i < Mathf.Min(4, defaultNormalSkill.Length); i++) {
            normalSkillId[i] = defaultNormalSkill[i].id;
        }
    }

    public PetSkill(PetSkill rhs) {
        id = rhs.id;
        ownSkill = rhs.ownSkill;
        normalSkill = rhs.normalSkill;
        superSkill = rhs.superSkill;
    }

    public LearnSkillInfo[] GetLearnSkillInfos(int[] skillIds) {
        return skillIds.Select(x => GetLearnSkillInfos(x)).ToArray();
    }

    public LearnSkillInfo GetLearnSkillInfos(int skillId) {
        return skillId == 0 ? null : info.learnInfoList.Find(x => x.skill.id == skillId);
    }

    public void CheckNewSkill(int level) {
        var newSkillId = info.learnInfoList.Where(x => !ownSkillId.Contains(x.skill.id)).Where(x => x.secretType == SecretType.GreaterThanLevel)
            .Where(x => level >= x.value).Select(x => x.skill.id);
        ownSkillId = ownSkillId.Concat(newSkillId).ToArray();
        superSkill = superSkill ?? ownSkill.FirstOrDefault(x => x.type == SkillType.必杀);

        if (!normalSkill.Contains(null))
            return;

        normalSkill = ownSkill.ToArray();
    }

    public void LearnNewSkill(Skill skill) {
        ownSkillId = ownSkillId.Concat(new int[1]{ skill.id }).ToArray();
    }

    // This will force normal skills and super skill to change.
    public void LearnAllSkill() {
        ownSkill = skillList;
        normalSkill = skillList.ToArray();
        superSkill = skillList.FirstOrDefault(x => x.type == SkillType.必杀);
    }

    public Skill[] GetNormalSkill() {
        Skill[] ret = normalSkillId.Select(x => Skill.GetSkill(x, false)).ToArray(); 
        Array.Resize(ref ret, 4);
        return ret;
    }

    public void SwapNormalSkill(Skill oldSkill, Skill newSkill) {
        if ((newSkill == null) || (ownSkillId == null))
            return;
        
        if ((!normalSkillId.Contains(oldSkill.id)) || (!ownSkillId.Contains(newSkill.id)))
            return;

        if (oldSkill.isSuper || oldSkill.isAction || newSkill.isSuper || newSkill.isAction)
            return;

        normalSkill = normalSkill.Update(oldSkill, newSkill).ToArray();
        SaveSystem.SaveData();
    }

    public void SwapSuperSkill() {
        if (backupSuperSkill != null) {
            superSkillId = backupSuperSkill.id;
            SaveSystem.SaveData();
        }
    }
}

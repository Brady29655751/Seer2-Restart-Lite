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
        get => ownSkillId.Distinct().Select(x => Skill.GetSkill(x, false)).Where(x => (x != null) && (!x.isAction)).ToList();
        set => ownSkillId = (value == null) ? new int[1] : value.Where(x => (x != null) && (!x.isAction)).Select(x => x.id).ToArray();
    }
    [XmlIgnore] public List<Skill> ownSuperSkill => ownSkill.Where(x => (x != null) && (x.type == SkillType.必杀)).ToList();
    
    /* Pet current skills */
    [XmlIgnore] public Skill[] normalSkill { 
        get => GetNormalSkill(); 
        set => normalSkillId = (value == null) ? new int[4] : value.Where(x => (x != null) && (!x.isAction) && (x.type != SkillType.必杀)).Take(4).Select(x => (x == null) ? 0 : x.id).ToArray();
    }
    public LearnSkillInfo[] normalSkillInfo => GetLearnSkillInfos(normalSkillId);

    [XmlIgnore] public Skill superSkill {
        get => Skill.GetSkill(superSkillId, false);
        set => superSkillId = (value == null) ? 0 : value.id;
    }
    public LearnSkillInfo superSkillInfo => GetLearnSkillInfos(superSkillId);

    /* Pet backup skills */
    public Skill[] backupNormalSkill => ownSkill.Where(x => (x != null) && (!x.isAction) && (x.type != SkillType.必杀) && 
        normalSkill.All(y => (x.id != (y?.id ?? 0)))).ToArray();
    
    public Skill backupSuperSkill => GetBackupSuperSkill();


    /* Pet secret skills */
    public int[] secretSkillId => secretSkill.Select(x => x.id).ToArray();
    public Skill[] secretSkill => info.learnInfoList.Where(x => (x.secretType != SecretType.GreaterThanLevel) && (x.secretType <= SecretType.SpecialPet)).Select(x => x.skill).ToArray();
    public LearnSkillInfo[] secretSkillInfo => GetLearnSkillInfos(secretSkillId);


    #endregion

    public PetSkill() {}

    public PetSkill(int _id, int _level, PetSkill originalPetSkill = null) {
        id = _id;

        ownSkill = originalPetSkill?.ownSkill ?? new List<Skill>();
        CheckNewSkill(_level);

        if (originalPetSkill == null)
            return;
        
        Skill[] defaultNormalSkill = originalPetSkill.normalSkill.Where(x => x != null).ToArray();
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
        if (skillId == 0)
            return null;

        var learnInfo = info.learnInfoList.Find(x => x.skill.id == skillId);

        // Cannot find info but own it => SecretType.Others
        if (learnInfo == null)
            return new LearnSkillInfo(Skill.GetSkill(skillId), -1);

        return learnInfo;
    }

    public void CheckNewSkill(int level) {
        var newSkillId = info.learnInfoList.Where(x => !ownSkillId.Contains(x.skill.id)).Where(x => x.secretType == SecretType.GreaterThanLevel)
            .Where(x => level >= x.value).Select(x => x.skill.id);
        ownSkillId = ownSkillId.Concat(newSkillId).Where(x => x != 0).ToArray();
        superSkill = superSkill ?? ownSkill?.FirstOrDefault(x => (x?.type ?? SkillType.属性) == SkillType.必杀);

        if (!normalSkill.Contains(null))
            return;

        normalSkill = ownSkill?.ToArray();
    }

    public bool LearnNewSkill(Skill skill) {
        if (skill == null)
            return false;

        if (ownSkillId.Contains(skill.id))
            return false;

        ownSkillId = ownSkillId.Concat(new int[1]{ skill.id }).ToArray();
        superSkill = superSkill ?? ownSkill.FirstOrDefault(x => x.type == SkillType.必杀);
        if (!normalSkill.Contains(null))
            return true;

        normalSkill = ownSkill.ToArray();
        return true;
    }

    // This will force normal skills and super skill to change.
    public void LearnAllSkill() {
        ownSkill = skillList;
        normalSkill = skillList.ToArray();
        superSkill = skillList.FirstOrDefault(x => x.type == SkillType.必杀);
    }

    public Skill[] GetNormalSkill() {
        Skill[] ret = normalSkillId.Distinct().Select(x => Skill.GetSkill(x, false)).Where(x => x != null)
            .Where(x => (x.type >= SkillType.属性) && (x.type != SkillType.必杀)).ToArray(); 
        Array.Resize(ref ret, 4);
        return ret;
    }

    public Skill GetBackupSuperSkill() {
        if(List.IsNullOrEmpty(ownSuperSkill) || (ownSuperSkill.Count == 1))
            return null;
        
        int id = ownSuperSkill.Select(x => x.id).IndexOf(superSkillId);
        if (id < 0)
            return null;

        return ownSuperSkill[(id + 1) % ownSuperSkill.Count];
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

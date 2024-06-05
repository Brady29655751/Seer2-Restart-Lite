using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInfo
{
    [XmlAttribute("id")] public int petId;
    [XmlAttribute("level")] public int level;
    public BattleStatus status = null;
    [XmlElement("emblem")] public bool hasEmblem = true;

    [XmlElement("initBuff")] public string initBuffIds;
    [XmlIgnore] public List<Buff> initBuffs => initBuffIds.ToIntList().Select(x => new Buff(x)).ToList();
    
    [XmlElement("headerSkill")] public string headerSkillIds;
    [XmlIgnore] public List<Skill> headerSkills => headerSkillIds.ToIntList().Select(x => Skill.GetSkill(x, false)).Where(x => x != null).ToList();
    
    [XmlElement("loopSkill")] public string loopSkillIds;
    [XmlIgnore] public List<Skill> loopSkills => loopSkillIds.ToIntList().Select(x => Skill.GetSkill(x, false)).Where(x => x != null).ToList();
    
    [XmlElement("normalSkill")] public string normalSkillIds;
    [XmlIgnore] public Skill[] normalSkills => normalSkillIds.ToIntList().Select(x => Skill.GetSkill(x, false)).ToArray();
    
    [XmlElement("superSkill")] public int superSkillId;
    [XmlIgnore] public Skill superSkill => Skill.GetSkill(superSkillId, false);

}

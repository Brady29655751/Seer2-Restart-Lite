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

    public static BossInfo GetRandomEnemyInfo(int level = 100, Func<Pet, bool> bannedPetFunc = null)
    {
        bannedPetFunc ??= (p) => false;
        var difficulty = YiTeRogueData.instance.difficulty;
        var petData = GameManager.versionData.petData;
        var petDict = petData.petLastEvolveDictionary;
        var petIdList = petDict.Where(x => !bannedPetFunc(x)).Select(x => x.id).ToList().Random(1);
        return GetRandomEnemyInfo(petIdList, level);
    }

    public static BossInfo GetRandomEnemyInfo(List<int> enemyIdList, int level = 100) 
    {
        var pet = Pet.GetExamplePet(enemyIdList.Random());
        pet.normalSkill = null;
        return new BossInfo()
        {
            petId = pet.id,
            level = level,
            loopSkillIds = pet.ownSkill.Where(x => !x.isSuper).Select(x => x.id.ToString()).ConcatToString(","),
            superSkillId = pet.ownSkill.FirstOrDefault(x => x.isSuper)?.id ?? 0,
            initBuffIds = "-3",
        };
    }

    public BossInfo FixToYiTeRogue(YiTeRogueEvent rogueEvent) {
        var floor = YiTeRogueData.instance.floor;
        var trace = YiTeRogueData.instance.trace.Count;
        var baseLevel = 80 + floor * YiTeRogueEvent.GetEndStepByFloor(floor) + (Mathf.Max(floor - 3, 0) * 5);
        var stepLevel = trace + rogueEvent.battleDifficulty * 5;
        var extraLevel = (rogueEvent.type == YiTeRogueEventType.End) ? 10 : stepLevel;
        var enemyLevel = baseLevel + extraLevel;
        var pet = Pet.GetExamplePet(petId, enemyLevel);

        level = enemyLevel;
        status = new BattleStatus(pet.normalStatus * (1 + Mathf.Max(0, floor - 2) / 5f)){ 
            hp = pet.normalStatus.hp * (floor + 1) 
        };
        hasEmblem = true;
        
        // 剔除煩人的特殊印記
        initBuffIds = initBuffIds.ToIntList().Update(13, 71).Update(14, 72)
            .Where(x => (x != -4) && (x > -3000) && (!x.IsInRange(21, 55)))
            .Append(-7).Select(x => x.ToString()).ConcatToString(",");

        // 技能和印记
        if (floor >= 3) {
            var endlessBuffs = new List<int>(){ 71, 72, -3022 };
            initBuffIds = initBuffIds.ToIntList().Union(endlessBuffs).Select(x => x.ToString()).ConcatToString(",");
            loopSkillIds = loopSkills.TakeLast(5).Append(loopSkills.Get(3, loopSkills.First())).Select(x => x.id.ToString()).ConcatToString(",");
            status.hit = 50;
        } else if (pet.element == Element.精灵王) {
            initBuffIds = initBuffIds.ToIntList().Append(3070).Select(x => x.ToString()).ConcatToString(",");
        }
        return this;
    }
}

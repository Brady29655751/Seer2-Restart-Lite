using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PetInfo
{
    public int id => basic.id;
    public int baseId => basic.baseId;
    public List<int> allEvolvePetIds => PetExpSystem.GetEvolveChain(baseId, id);
    public string name => basic.name;
    public int star => ui.star;

    public PetBasicInfo basic;
    public PetFeatureInfo feature;
    public PetExpInfo exp;
    public PetTalentInfo talent;
    public PetSkillInfo skills;
    public PetUIInfo ui;

    public static bool IsMod(int id) => (id < -12);
    public static List<PetInfo> database => Database.instance.petInfoDict.Select(x => x.Value).Where(x => x != null)
        .OrderByDescending(x => x.id.GetSortPriority()).ToList();

    public PetInfo() {}

    public PetInfo(PetBasicInfo basicInfo, PetFeatureInfo featureInfo, PetExpInfo expInfo, PetTalentInfo talentInfo, PetSkillInfo skillInfo, PetUIInfo uiInfo) {
        basic = basicInfo;
        feature = featureInfo;
        exp = expInfo;
        talent = talentInfo;
        skills = skillInfo;
        ui = uiInfo;
    }
}



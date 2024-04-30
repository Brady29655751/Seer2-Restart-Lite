using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetInfo
{
    public int id => basic.id;
    public int baseId => basic.baseId;
    public List<int> allEvolvePetIds => basic.allEvolvePetIds;
    public string name => basic.name;

    public PetBasicInfo basic;
    public PetFeatureInfo feature;
    public PetExpInfo exp;
    public PetTalentInfo talent;
    public PetSkillInfo skills;
    public PetUIInfo ui;

    public static bool IsMod(int id) => (id < -12);

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



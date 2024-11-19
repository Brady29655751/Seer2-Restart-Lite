using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetAdvanceModel : Module
{
    [SerializeField] private IInputField baseIdInputField, evolveIdInputField, evolveLevelInputField;
    [SerializeField] private IDropdown expTypeDropdown;
    [SerializeField] private IInputField featureInputField, featureDescriptionInputField;
    [SerializeField] private IInputField emblemInputField, emblemDescriptionInputField;
    

    public int baseId => string.IsNullOrEmpty(baseIdInputField.inputString) ? 0 : (int.TryParse(baseIdInputField.inputString, out var resultId) ? resultId : 0);
    public List<int> evolveId => evolveIdInputField.inputString.ToIntList('/');
    public List<int> evolveLevel => evolveLevelInputField.inputString.ToIntList('/');
    public int expType => expTypeDropdown.value;
    public string feature => featureInputField.inputString;
    public string featureDescription => featureDescriptionInputField.inputString;
    public string emblem => emblemInputField.inputString;
    public string emblemDescription => emblemDescriptionInputField.inputString;

    public List<LearnSkillInfo> learnSkillInfoList = new List<LearnSkillInfo>();


    public PetFeatureInfo GetPetFeatureInfo(int id) {
        Feature petFeature = new Feature() {
            baseId = id,
            name = feature.ReplaceSpecialWhiteSpaceCharacters(string.Empty),
            rawDescription = featureDescription.ReplaceSpecialWhiteSpaceCharacters(string.Empty),
        };
        Emblem petEmblem = new Emblem() {
            baseId = id,
            name = emblem.ReplaceSpecialWhiteSpaceCharacters(string.Empty),
            rawDescription = emblemDescription.ReplaceSpecialWhiteSpaceCharacters(string.Empty),
        };
        return new PetFeatureInfo(id, petFeature, petEmblem);
    }

    public PetExpInfo GetPetExpInfo(int id) {
        return new PetExpInfo(id, expType, evolveId, evolveLevel, 0);
    }

    public PetSkillInfo GetPetSkillInfo(int id) {
        return new PetSkillInfo(id, learnSkillInfoList);
    }

    public void SetPetFeatureInfo(int baseId, PetFeatureInfo featureInfo) {
        baseIdInputField.SetInputString(baseId.ToString());
        featureInputField.SetInputString(featureInfo.feature.name);
        featureDescriptionInputField.SetInputString(featureInfo.feature.description);
        emblemInputField.SetInputString(featureInfo.emblem.name);
        emblemDescriptionInputField.SetInputString(featureInfo.emblem.description);
    }    

    public void SetPetExpInfo(PetExpInfo expInfo) {
        evolveIdInputField.SetInputString(expInfo.evolvePetIds.Select(x => x.ToString()).ConcatToString("/"));
        evolveLevelInputField.SetInputString(expInfo.evolveLevels.Select(x => x.ToString()).ConcatToString("/"));
        expTypeDropdown.value = expInfo.expType;
    }

    public void SetPetSkillInfo(PetSkillInfo skillInfo) {
        learnSkillInfoList = skillInfo.skillIdList.Select(skillId => Skill.GetSkill(skillId, false)).Where(x => x != null)
            .Zip(skillInfo.learnLevelList, (skill, level) => new LearnSkillInfo(skill, level)).ToList();
    }

    public void OnAddSkill(LearnSkillInfo info) {
        learnSkillInfoList.Add(info);
    }

    public void OnRemoveSkill() {
        if (learnSkillInfoList.Count == 0)
            return;

        learnSkillInfoList.RemoveAt(learnSkillInfoList.Count - 1);
    }

    public void OnSelectFeature(BuffInfo info) {
        featureInputField.SetInputString(info.name);
        featureDescriptionInputField.SetInputString(info.description);
    }

    public void OnSelectEmblem(BuffInfo info) {
        emblemInputField.SetInputString(info.name);
        emblemDescriptionInputField.SetInputString(info.description);
    }

    public bool VerifyDIYPetAdvance(int id, bool useDefaultFeature, out string error) {
        error = string.Empty;

        if (!VerifyBaseId(id, out error))
            return false;

        if (!VerifyExpInfo(id, out error))
            return false;

        if (!VerifySkillInfo(id, out error))
            return false;

        if ((!useDefaultFeature) && (!VerifyFeatureInfo(id, out error)))
            return false;

        return true;
    }

    private bool VerifyBaseId(int id, out string error) {
        error = string.Empty;

        if (baseId == 0) {
            error = "基础型态序号不能为空！";
            return false;
        }

        if ((id != baseId) && (Pet.GetPetInfo(baseId) == null)) {
            error = "基础型态序号不存在";
            return false;
        }

        return true;
    }

    private bool VerifyExpInfo(int id, out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(evolveIdInputField.inputString) || string.IsNullOrEmpty(evolveLevelInputField.inputString)) {
            error = "进化型态序号和等级不能为空！";
            return false;
        }

        var evolveIdList = evolveIdInputField.inputString.Split('/');
        var evolveLevelList = evolveLevelInputField.inputString.Split('/');

        if (evolveIdList.Length != evolveLevelList.Length) {
            error = "进化型态数量和进化等级数量不一致";
            return false;
        }

        for (int i = 0; i < evolveIdList.Length; i++) {
            if (!int.TryParse(evolveIdList[i], out _)) {
                error = "进化型态序号需为整数！";
                return false;
            }
        }

        for (int i = 0; i < evolveLevelList.Length; i++) {
            if (!int.TryParse(evolveLevelList[i], out var elv)) {
                error = "进化等级需为整数！";
                return false;
            }

            if (elv < 0) {
                error = "进化等级不能为负数！";
                return false;
            }
        }

        if (evolveId.Contains(id)) {
            error = "进化型态序号产生回圈！";
            return false;
        }

        return true;
    }

    private bool VerifyFeatureInfo(int id, out string error) {
        error = string.Empty;

        if (id != baseId)
            return true;

        if (string.IsNullOrEmpty(feature) || string.IsNullOrEmpty(featureDescription)) {
            error = "特性名称及描述不能为空！";
            return false;
        }

        if (feature.Contains(',') || featureDescription.Contains(',')) {
            error = "特性名称及描述不能有半形逗号";
            return false;
        }

        if (string.IsNullOrEmpty(emblem) || string.IsNullOrEmpty(emblemDescription)) {
            error = "纹章名称及描述不能为空！";
            return false;
        }

        if (emblem.Contains(',') || emblemDescription.Contains(',')) {
            error = "纹章名称及描述不能有半形逗号";
            return false;
        }

        var featureBuffInfo = Buff.GetBuffInfo(90_0000 + Mathf.Abs(baseId));
        var emblemBuffInfo = Buff.GetBuffInfo(80_0000 + Mathf.Abs(baseId));

        if ((featureBuffInfo == null) || ((emblem != "none") && (emblemBuffInfo == null))) {
            error = "请先制作对应的特性和纹章印记！";
            return false;
        }

        if ((featureBuffInfo.name != feature) || ((emblem != "none") && (emblemBuffInfo.name != emblem))) {
            error = "特性、纹章的名称和对应之印记名称不符";
            return false;
        }

        return true;
    }

    private bool VerifySkillInfo(int id, out string error) {
        error = string.Empty;

        /*
        if (ListHelper.IsNullOrEmpty(learnSkillInfoList)) {
            error = "技能不能为空！";
            return false;
        }
        */

        if ((id == baseId) && (!learnSkillInfoList.Any(x => x.skill.type != SkillType.必杀))) {
            error = "基础型态至少需要一个非必杀技能";
            return false;
        }

        return true;
    }

}

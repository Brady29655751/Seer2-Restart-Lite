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
    public int evolveId => int.Parse(evolveIdInputField.inputString);
    public int evolveLevel => int.Parse(evolveLevelInputField.inputString);
    public int expType => expTypeDropdown.value;
    public string feature => featureInputField.inputString;
    public string featureDescription => featureDescriptionInputField.inputString;
    public string emblem => emblemInputField.inputString;
    public string emblemDescription => emblemDescriptionInputField.inputString;

    public List<LearnSkillInfo> learnSkillInfoList = new List<LearnSkillInfo>();


    public PetFeatureInfo GetPetFeatureInfo(int id) {
        Feature petFeature = new Feature() {
            baseId = id,
            name = feature,
            description = featureDescription,
        };
        Emblem petEmblem = new Emblem() {
            baseId = id,
            name = emblem,
            description = emblemDescription,
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
        evolveIdInputField.SetInputString(expInfo.evolvePetId.ToString());
        evolveLevelInputField.SetInputString(expInfo.evolveLevel.ToString());
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

        if (!VerifySkillInfo(out error))
            return false;

        if ((!useDefaultFeature) && (id == baseId) && (!VerifyFeatureInfo(out error)))
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

        if (!int.TryParse(evolveIdInputField.inputString, out _)) {
            error = "进化型态序号需为整数！";
            return false;
        }

        if ((id == evolveId) || IsEvolveLoop(id)) {
            error = "进化型态序号产生回圈！";
            return false;
        }

        if (evolveLevel < 0) {
            error = "进化等级不能为负数！";
            return false;
        }

        return true;
    }

    private bool IsEvolveLoop(int id) {
        if (Pet.GetPetInfo(evolveId) == null)
            return false;

        var petInfo = Pet.GetPetInfo(evolveId);
        while (petInfo != null) {
            if (petInfo.exp.evolvePetId == id)
                return true;

            petInfo = Pet.GetPetInfo(petInfo.exp.evolvePetId);
        }
        return false;
    }

    private bool VerifyFeatureInfo(out string error) {
        error = string.Empty;

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

    private bool VerifySkillInfo(out string error) {
        error = string.Empty;

        if (List.IsNullOrEmpty(learnSkillInfoList)) {
            error = "技能不能为空！";
            return false;
        }

        if (!learnSkillInfoList.Any(x => x.skill.type != SkillType.必杀)) {
            error = "至少需要一个非必杀技能";
            return false;
        }

        return true;
    }

}

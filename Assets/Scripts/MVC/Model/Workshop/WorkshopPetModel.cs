using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetModel : SelectModel<GameObject>
{
    [SerializeField] private WorkshopPetBasicModel petBasicModel;
    [SerializeField] private WorkshopPetAdvanceModel petAdvanceModel;
    [SerializeField] private WorkshopPetSkinModel petSkinModel;

    public event Action<string, Sprite> onUploadSpriteEvent;
    public PetInfo petInfo => GetPetInfo();

    public PetInfo GetPetInfo() {
        PetBasicInfo basicInfo = petBasicModel.GetPetBasicInfo(petAdvanceModel.baseId);
        PetFeatureInfo featureInfo = petAdvanceModel.GetPetFeatureInfo(petBasicModel.id);
        PetExpInfo expInfo = petAdvanceModel.GetPetExpInfo(petBasicModel.id);
        PetSkillInfo skillInfo = petAdvanceModel.GetPetSkillInfo(petBasicModel.id);
        PetUIInfo uiInfo = petSkinModel.GetPetUIInfo(petBasicModel.id, petAdvanceModel.baseId);

        PetInfo info = new PetInfo(basicInfo, featureInfo, expInfo, new PetTalentInfo(), skillInfo, uiInfo);
        return info;
    }

    public void SetPetInfo(PetInfo petInfo) {
        petBasicModel.SetPetBasicInfo(petInfo.basic);
        petAdvanceModel.SetPetFeatureInfo(petInfo.baseId, PetFeature.GetFeatureInfo(petInfo.id) ?? petInfo.feature);
        petAdvanceModel.SetPetExpInfo(petInfo.exp);
        petAdvanceModel.SetPetSkillInfo(petInfo.skills);
        petSkinModel.SetPetUIInfo(petInfo.ui);
    }

    public void OnUploadSprite(string type) {
        petSkinModel.OnUploadSprite(type, OnUploadSpriteSuccess);
    }

    private void OnUploadSpriteSuccess(Sprite sprite) {
        onUploadSpriteEvent?.Invoke(petSkinModel.spriteType, petSkinModel.spriteDict[petSkinModel.spriteType]);
    }

    public void OnClearSprite(string type) {
        petSkinModel.OnClearSprite(type);
    }

    public void OnAddSkill(LearnSkillInfo info) {
        petAdvanceModel.OnAddSkill(info);
    }

    public void OnRemoveSkill() {
        petAdvanceModel.OnRemoveSkill();
    }

    public void OnSelectFeature(BuffInfo info) {
        petAdvanceModel.OnSelectFeature(info);
    }

    public void OnSelectEmblem(BuffInfo info) {
        petAdvanceModel.OnSelectEmblem(info);
    }

    public bool CreateDIYPet() {
        var originalPetInfo = Pet.GetPetInfo(petInfo.id);
        var originalFeatureInfo = PetFeature.GetFeatureInfo(petInfo.id);

        Database.instance.featureInfoDict.Set(petInfo.id, petInfo.feature);

        var petFeatureInfo = PetFeature.GetFeatureInfo(petInfo.ui.defaultFeatureId);
        var newPetInfo = new PetInfo(petInfo.basic, petFeatureInfo, petInfo.exp, petInfo.talent, petInfo.skills, petInfo.ui);

        Database.instance.petInfoDict.Set(petInfo.id, newPetInfo);

        if (SaveSystem.TrySavePetMod(petInfo, petSkinModel.bytesDict, petSkinModel.spriteDict))
            return true;

        // rollback
        Database.instance.featureInfoDict.Set(petInfo.id, originalFeatureInfo);
        Database.instance.petInfoDict.Set(petInfo.id, originalPetInfo);
        return false;
    }

    public bool DeleteDIYPet(out string message) {
        if (!petBasicModel.VerifyDIYPetBasic(petAdvanceModel.baseId, out message))
            return false;

        var originalPetInfo = Pet.GetPetInfo(petBasicModel.id);
        var originalFeatureInfo = PetFeature.GetFeatureInfo(petBasicModel.id);

        if ((originalPetInfo == null) || (!PetInfo.IsMod(petBasicModel.id))) {
            message = "未检测到此序号的Mod精灵";
            return false;
        }

        /*
        if ((originalFeatureInfo == null) || (!PetInfo.IsMod(petBasicModel.id))) {
            message = "未检测到此序号的Mod特性";
            return false;
        }
        */

        Database.instance.petInfoDict.Remove(petBasicModel.id);
        Database.instance.featureInfoDict.Remove(petBasicModel.id);
        if (SaveSystem.TrySavePetMod(originalPetInfo, null, null, petBasicModel.id)) {
            message = "精灵删除成功";
            return true;
        }

        // rollback
        Database.instance.featureInfoDict.Set(petInfo.id, originalFeatureInfo);
        Database.instance.petInfoDict.Set(petInfo.id, originalPetInfo);
        message = "精灵删除失败（档案写入问题）";
        return false;
    }

    public bool VerifyDIYPet(out string error) {
        error = string.Empty;

        if (!petBasicModel.VerifyDIYPetBasic(petAdvanceModel.baseId, out error))
            return false;

        if (!petAdvanceModel.VerifyDIYPetAdvance(petBasicModel.id, petSkinModel.options.Contains("default_feature="), out error))
            return false;

        if (!petSkinModel.VerifyDIYPetSkin(petBasicModel.id, petAdvanceModel.baseId, out error))
            return false;

        return true;
    }

}

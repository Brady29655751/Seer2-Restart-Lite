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
        petAdvanceModel.SetPetFeatureInfo(petInfo.feature);
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

    public bool CreateDIYPet() {
        var originalPetInfo = Pet.GetPetInfo(petInfo.id);
        Database.instance.petInfoDict.Set(petInfo.id, petInfo);
        if (SaveSystem.TrySavePetMod(petInfo, petSkinModel.bytesDict))
            return true;

        // rollback
        Database.instance.petInfoDict.Set(petInfo.id, originalPetInfo);
        return false;
    }

    public bool VerifyDIYPet(out string error) {
        error = string.Empty;

        if (!petBasicModel.VerifyDIYPetBasic(petAdvanceModel.baseId, out error))
            return false;

        if (!petAdvanceModel.VerifyDIYPetAdvance(petBasicModel.id, out error))
            return false;

        if (!petSkinModel.VerifyDIYPetSkin(petBasicModel.id, petAdvanceModel.baseId, out error))
            return false;

        return true;
    }

}

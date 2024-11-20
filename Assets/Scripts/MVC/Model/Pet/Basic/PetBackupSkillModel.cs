using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBackupSkillModel : SelectModel<Skill>
{
    [SerializeField] private bool special = false;
    public bool activeSelf { get; protected set; } = false;

    public Pet currentPet { get; protected set; }
    public Skill[] normalSkills => selections;
    public LearnSkillInfo[] normalSkillInfos => currentPet?.skills.GetLearnSkillInfos(normalSkills.Select(x => x == null ? 0 : x.id).ToArray());
    public Skill superSkill { get; protected set; }
    public LearnSkillInfo superSkillInfo => (superSkill == null) ? null : currentPet?.skills.GetLearnSkillInfos(superSkill.id);

    public Skill currentSelectedNormalSkill => (isNormalSkillChosen ? 
        selectableArray.currentSelectItems[0] : null);
    public Skill currentSelectedSuperSkill => (isSuperSkillChosen ?
        superSkill : null);

    public int[] normalSkillCursor => cursor;
    public bool isNormalSkillChosen => normalSkillCursor.Length > 0;
    public bool isSuperSkillChosen { get; private set; } = false;

    public override int GetLastPage()
    {
        return (currentPet == null) ? 0 : base.GetLastPage();
    }

    protected override void SetSelections(Skill[] selections)
    {
        Array.Resize(ref selections, capacity);
        base.SetSelections(selections);
    }

    public void SetPet(Pet pet, int defaultSelectPage = 0, bool active = false) {
        currentPet = pet;
        SetActive(active);

        if (currentPet == null)
            return;

        SetStorage(currentPet.backupNormalSkill.ToList(), defaultSelectPage);
        Filter(SpecialFilter, defaultSelectPage);
        superSkill = currentPet.skills.GetBackupSuperSkill(SpecialFilter);
    }

    public bool SpecialFilter(Skill skill) {
        var skillInfo = currentPet.skills.GetLearnSkillInfos(skill?.id ?? 0);
        if (skillInfo == null)
            return false;

        return special ^ (skillInfo.secretType != SecretType.Others);
    }

    public void SelectNormalSkill(int index) {
        if (index < 0)
            selectableArray.InitSelectedFlag(false);

        if (!index.IsInRange(0, capacity))
            return;

        selectableArray.Select(index);
    }

    public void SelectSuperSkill() {
        isSuperSkillChosen = !isSuperSkillChosen;
    }

    public void SwapPetNormalSkill(Skill currentSkill) {
        if (!isNormalSkillChosen)
            return;
        
        currentPet.skills.SwapNormalSkill(currentSkill, currentSelectedNormalSkill);
        if (!SpecialFilter(currentSkill))
            return;
            
        Replace(currentSkill, normalSkillCursor[0]);
        SetPage(page);
    }

    public void SwapPetSuperSkill(Skill currentSkill) {
        currentPet.skills.SwapSuperSkill(currentSelectedSuperSkill);
        isSuperSkillChosen = false;
        superSkill = currentPet.skills.GetBackupSuperSkill(SpecialFilter);
    }

    public void SetActive(bool active) {
        activeSelf = active;
        if (!activeSelf) {
            isSuperSkillChosen = false;
            SetPage(page);
        }
    }
}

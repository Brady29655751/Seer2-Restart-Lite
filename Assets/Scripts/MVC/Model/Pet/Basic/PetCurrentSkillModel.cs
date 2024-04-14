using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCurrentSkillModel : SelectModel<Skill>
{
    public Pet currentPet;

    public Skill[] normalSkills => selections;
    public LearnSkillInfo[] normalSkillInfos => normalSkills.Select(x => (x == null) ? null : currentPet.skills.GetLearnSkillInfos(x.id)).ToArray();
    public Skill superSkill { get; protected set; }
    public LearnSkillInfo superSkillInfo => superSkill == null ? null : currentPet.skills.GetLearnSkillInfos(superSkill.id);

    public Skill currentSelectedNormalSkill => (isNormalSkillChosen ? 
        selectableArray.currentSelectItems[0] : null);
    public Skill currentSelectedSuperSkill => (isSuperSkillChosen ?
        superSkill : null);

    public int[] normalSkillCursor => cursor;

    public bool isNormalSkillChosen => normalSkillCursor.Length > 0;
    public bool isSuperSkillChosen { get; private set; } = false;

    public void SetPet(Pet pet, int defaultSelectPage = 0) {
        currentPet = pet;
        isSuperSkillChosen = false;

        if (currentPet == null)
            return;

        SetStorage(currentPet.normalSkill.ToList(), defaultSelectPage);
        superSkill = currentPet.superSkill;
    }

    public void SelectNormalSkill(int index) {
        if (!index.IsInRange(0, selectableArray.capacity))
            return;
    
        selectableArray.Select(index);
    }
    
    public void SelectSuperSkill() {
        isSuperSkillChosen = !isSuperSkillChosen;
    }

    public void SwapPetNormalSkill(Skill backupSkill) {
        if (!isNormalSkillChosen)
            return;

        Replace(backupSkill, normalSkillCursor[0]);
        SetPage(page);
    }

    public void SwapPetSuperSkill(Skill backupSkill) {
        isSuperSkillChosen = false;
        superSkill = backupSkill ?? superSkill;
    }

}

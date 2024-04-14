using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSwapSkillController : Module
{
    [SerializeField] private PetCurrentSkillController currentSkillController;
    [SerializeField] private PetBackupSkillController backupSkillController;

    protected override void Awake()
    {
        base.Awake();
        InitSwapSkillSubscriptions();
    }

    private void InitSwapSkillSubscriptions() {
        currentSkillController.onSelectNormalSkillEvent += OnSelectCurrentNormalSkill;
        currentSkillController.onSelectSuperSkillEvent += OnSelectCurrentSuperSkill;
        backupSkillController.onSelectNormalSkillEvent += OnSelectBackupNormalSkill;
        backupSkillController.onSelectSuperSkillEvent += OnSelectBackupSuperSkill;
    }

    public void SetPet(Pet pet) {
        currentSkillController.SetPet(pet);
        backupSkillController.SetPet(pet);
    }

    public void OnSelectCurrentNormalSkill(Skill currentSkill) {
        Skill backupSkill = backupSkillController.SwapNormalSkill(currentSkill);
        currentSkillController.SwapNormalSkill(backupSkill);
    }

    public void OnSelectCurrentSuperSkill(Skill currentSkill) {
        Skill backupSkill = backupSkillController.SwapSuperSkill(currentSkill);
        currentSkillController.SwapSuperSkill(backupSkill);
    }

    public void OnSelectBackupNormalSkill(Skill backupSkill) {
        Skill currentSkill = currentSkillController.SwapNormalSkill(backupSkill);
        backupSkillController.SwapNormalSkill(currentSkill);
    }

    public void OnSelectBackupSuperSkill(Skill backupSkill) {
        Skill currentSkill = currentSkillController.SwapSuperSkill(backupSkill);
        backupSkillController.SwapSuperSkill(currentSkill);
    }

}

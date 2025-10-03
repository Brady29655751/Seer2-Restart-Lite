using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSwapSkillController : Module
{
    [SerializeField] private PetCurrentSkillController currentSkillController;
    [SerializeField] private PetBackupSkillController backupSkillController;
    [SerializeField] private PetBackupSkillController specialSkillController;

    private bool isNormalSwapperSpecial => backupSkillController.currentSelectedNormalSkill == null;
    private PetBackupSkillController normalSkillSwapper => isNormalSwapperSpecial  ? specialSkillController : backupSkillController;
    private PetBackupSkillController normalSkillHolder => isNormalSwapperSpecial ? backupSkillController : specialSkillController;

    private bool isSuperSwapperSpecial => backupSkillController.currentSelectedSuperSkill == null;
    private PetBackupSkillController superSkillSwapper => isSuperSwapperSpecial ? specialSkillController : backupSkillController;
    private PetBackupSkillController superSkillHolder => isSuperSwapperSpecial ? backupSkillController : specialSkillController;


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
        specialSkillController.onSelectNormalSkillEvent += OnSelectSpecialNormalSkill;
        specialSkillController.onSelectSuperSkillEvent += OnSelectSpecialSuperSkill;
    }

    public void SetRule(BattleRule rule)
    {
        currentSkillController.Rule = rule;
        backupSkillController.Rule = rule;
        specialSkillController.Rule = rule;
    }

    public void SetPet(Pet pet)
    {
        currentSkillController.SetPet(pet);
        backupSkillController.SetPet(pet);
        specialSkillController.SetPet(pet);
    }

    public void OnSelectCurrentNormalSkill(Skill currentSkill) {
        var isCurrentSkillForSwapper = normalSkillSwapper.Filter(currentSkill);
        Skill backupSkill = normalSkillSwapper.SwapNormalSkill(currentSkill);
        currentSkillController.SwapNormalSkill(backupSkill);

        if ((backupSkill == null) || isCurrentSkillForSwapper)
            return;
        
        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

    public void OnSelectCurrentSuperSkill(Skill currentSkill) {
        var isCurrentSkillForSwapper = superSkillSwapper.Filter(currentSkill);
        Skill backupSkill = superSkillSwapper.SwapSuperSkill(currentSkill);
        currentSkillController.SwapSuperSkill(backupSkill);

        if ((backupSkill == null) || isCurrentSkillForSwapper)
            return;
        
        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

    public void OnSelectBackupNormalSkill(Skill backupSkill) {
        specialSkillController.SelectNormalSkill(-1);

        Skill currentSkill = currentSkillController.SwapNormalSkill(backupSkill);
        backupSkillController.SwapNormalSkill(currentSkill);

        if ((currentSkill == null) || backupSkillController.Filter(currentSkill))
            return;

        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

    public void OnSelectBackupSuperSkill(Skill backupSkill) {
        if (specialSkillController.currentSelectedSuperSkill != null)
            specialSkillController.SelectSuperSkill();

        Skill currentSkill = currentSkillController.SwapSuperSkill(backupSkill);
        backupSkillController.SwapSuperSkill(currentSkill);

        if ((currentSkill == null) || backupSkillController.Filter(currentSkill))
            return;
        
        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

    public void OnSelectSpecialNormalSkill(Skill specialSkill) {
        backupSkillController.SelectNormalSkill(-1);

        Skill currentSkill = currentSkillController.SwapNormalSkill(specialSkill);
        specialSkillController.SwapNormalSkill(currentSkill);

        if ((currentSkill == null) || specialSkillController.Filter(currentSkill))
            return;

        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

    public void OnSelectSpecialSuperSkill(Skill specialSkill) {
        if (backupSkillController.currentSelectedSuperSkill != null)
            backupSkillController.SelectSuperSkill();

        Skill currentSkill = currentSkillController.SwapSuperSkill(specialSkill);
        specialSkillController.SwapSuperSkill(currentSkill);

        if ((currentSkill == null) || specialSkillController.Filter(currentSkill))
            return;

        backupSkillController.Refresh();
        specialSkillController.Refresh();
    }

}

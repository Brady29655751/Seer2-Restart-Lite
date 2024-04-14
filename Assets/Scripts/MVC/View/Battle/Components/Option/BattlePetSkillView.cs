using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetSkillView : BattleBaseView
{
    private BattlePet pet;
    [SerializeField] private IButton superSkillButton;
    [SerializeField] private Image[] superSkillButtonBackground = new Image[3];
    [SerializeField] private BattlePetSkillBlockView[] skillBlockViews = new BattlePetSkillBlockView[4];

    public void SetPet(BattlePet pet) {
        if (pet == null)
            return;
            
        this.pet = pet;
        SetNormalSkill();
    }

    public void SetInteractable(bool interactable) {
        int anger = (interactable && !pet.isDead) ? pet.anger : -1;
        SetNormalSkillInteractable(anger);
        SetSuperSkillInteractable(anger);
    }

    private void SetNormalSkill() {
        for (int i = 0; i < pet.normalSkill.Length; i++) {
            skillBlockViews[i].SetSkill(pet.normalSkill[i]);
        }
    }

    private void SetNormalSkillInteractable(int petAnger) {
        for (int i = 0; i < pet.normalSkill.Length; i++) {
            bool interactable = (pet.normalSkill[i] == null) ? false : (petAnger >= pet.normalSkill[i].anger);
            skillBlockViews[i].SetInteractable(interactable);
        }
    }

    private void SetSuperSkillInteractable(int petAnger) {
        bool interactable = (pet.superSkill != null) && (petAnger >= pet.superSkill.anger);
        superSkillButton.SetInteractable(interactable);
        superSkillButtonBackground[1].gameObject.SetActive(interactable);
        superSkillButtonBackground[2].gameObject.SetActive(interactable);
    }

    public void SelectNormalSkill(int index) {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        battle.SetSkill(pet.normalSkill[index], true);
    }

    public void ShowNormalSkillInfo(int index) {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        float boxSizeY = Mathf.Max(150, (pet?.normalSkill[index] != null) ? pet.normalSkill[index].description.GetPreferredSize(12, 12, 21, 35).y : 150);
        descriptionBox.SetBoxSize(new Vector2(170, boxSizeY));
        descriptionBox.SetBoxPosition(new Vector2(80 + 175 * index, 109));
        descriptionBox.SetText(pet?.normalSkill[index].description);
    }

    public void SelectSuperSkill() {
        if (pet.superSkill == null)
            return;
            
        battle.SetSkill(pet.superSkill, true);
    }

    public void ShowSuperSkillInfo() {
        float boxSizeY = Mathf.Max(150, (pet?.superSkill != null) ? pet.superSkill.description.GetPreferredSize(12, 14).y : 150);
        descriptionBox.SetBoxSize(new Vector2(170, boxSizeY));
        descriptionBox.SetBoxPosition(new Vector2(5, 115));
        descriptionBox.SetText((pet?.superSkill != null) ? pet.superSkill.description : "尚未习得必杀技");
    }

}

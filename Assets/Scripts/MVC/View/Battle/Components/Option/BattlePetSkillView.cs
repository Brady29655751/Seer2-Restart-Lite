using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetSkillView : BattleBaseView
{
    private UnitPetSystem petSystem;
    private BattlePet pet => petSystem?.pet;
    private bool interactable;
    private bool superSkillClickable = false;
    private bool evolve = false;

    [SerializeField] private Material shiningMaterial;
    [SerializeField] private IButton noOpSkillButton, superSkillButton, evolveSkillButton;
    [SerializeField] private Image[] superSkillButtonBackground = new Image[3];
    [SerializeField] private BattlePetSkillBlockView[] skillBlockViews = new BattlePetSkillBlockView[4];

    public void SetPetSystem(UnitPetSystem petSystem) {
        if (petSystem?.pet == null)
            return;
            
        this.petSystem = petSystem;
        SetNormalSkill();
        SetInteractable(interactable);
    }

    public void SetInteractable(bool interactable) {
        var anger = -1;

        if (interactable && (!pet.isDead))
            anger = (pet.buffController.GetBuff(61) != null) ? int.MaxValue : pet.anger;

        SetNormalSkillInteractable(anger);
        SetSuperSkillInteractable(anger);
        SetNoOpSkillInteractable((anger == -1) ? int.MaxValue : anger);
        SetEvolveSkillInteractable(interactable && (petSystem.specialChance > 0));

        this.interactable = interactable;
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
        superSkillClickable = interactable;
        superSkillButton.SetInteractable(true);
        superSkillButtonBackground[0].SetMaterial(interactable ? shiningMaterial : null);
        // superSkillButtonBackground[1].gameObject.SetActive(interactable);
        // superSkillButtonBackground[2].gameObject.SetActive(interactable);
    }

    private void SetEvolveSkillInteractable(bool interactable) {
        evolveSkillButton?.SetInteractable(interactable);
        if (!interactable)
            evolveSkillButton?.SetMaterial(null);
    }

    public void SelectNormalSkill(int index) {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        var skill = new Skill(pet.normalSkill[index]);
        if (battle.settings.parallelCount > 1)
            skill.SetParallelIndex(battle.currentState.myUnit.petSystem.cursor, battle.currentState.opUnit.petSystem.cursor);

        if (evolve) {
            skill.options.Set("evolve", "1");
            evolve = false;
        }

        battle.SetSkill(skill, true);
    }

    public void ShowNormalSkillInfo(int index) {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        float boxSizeY = Mathf.Max(150, pet?.normalSkill[index]?.description.GetPreferredSize(12, 12, 21, 35).y ?? 0);
        float boxSizeYMedium = Mathf.Max(150, pet?.normalSkill[index]?.description.GetPreferredSize(15, 12, 21, 35).y ?? 0);
        float boxSizeYLarge = Mathf.Max(150, pet?.normalSkill[index]?.description.GetPreferredSize(24, 12, 21, 35).y ?? 0);
        var boxSize = (boxSizeY > 250) ? ((boxSizeYMedium > 300) ? new Vector2(300, boxSizeYLarge) : new Vector2(200, boxSizeYMedium)) : new Vector2(170, boxSizeY);
        descriptionBox.SetBoxSize(boxSize);
        descriptionBox.SetText(pet?.normalSkill[index].description);
        descriptionBox.SetBoxPosition(new Vector2(80 + 175 * index, 109));
    }

    public void SelectSuperSkill() {
        if (pet.superSkill == null)
            return;

        if (!superSkillClickable)
            return;

        var skill = new Skill(pet.superSkill);
        if (battle.settings.parallelCount > 1)
            skill.SetParallelIndex(battle.currentState.myUnit.petSystem.cursor, battle.currentState.opUnit.petSystem.cursor);

        if (evolve) {
            skill.options.Set("evolve", "1");
            evolve = false;
        }

        battle.SetSkill(skill, true);
    }

    public void ShowSuperSkillInfo() {
        float boxSizeY = Mathf.Max(150, pet?.superSkill?.description.GetPreferredSize(12, 14).y ?? 0);
        float boxSizeYMedium = Mathf.Max(150, pet?.superSkill?.description.GetPreferredSize(15, 14).y ?? 0);
        float boxSizeYLarge = Mathf.Max(150, pet?.superSkill?.description.GetPreferredSize(24, 14).y ?? 0);
        var boxSize = (boxSizeY > 250) ? ((boxSizeYMedium > 300) ? new Vector2(300, boxSizeYLarge) : new Vector2(200, boxSizeYMedium)) : new Vector2(170, boxSizeY);
        descriptionBox.SetBoxSize(boxSize);
        descriptionBox.SetText((pet?.superSkill != null) ? pet.superSkill.description : "尚未习得必杀技");
        descriptionBox.SetBoxPosition(new Vector2(5, 115));
    }

    public void SetNoOpSkillInteractable(int petAnger) {
        bool isNormalSkillUsable = pet.normalSkill.Where(x => x != null).Any(x => petAnger >= x.anger);
        bool isSuperSkillUsable = (pet.superSkill != null) && (petAnger >= pet.superSkill.anger);
        bool isAnySkillUsable = isNormalSkillUsable || isSuperSkillUsable;
        noOpSkillButton.SetInteractable(!isAnySkillUsable);
    }

    public void SelectNoOpSkill() {
        battle.SetSkill(Skill.GetNoOpSkill(), true);
    }

    public void ShowNoOpSkillInfo() {
        infoPrompt.SetSkill(Skill.GetNoOpSkill(), false);
    }

    public void SelectEvolveSkill() {
        if (petSystem.specialChance <= 0)
            return;
        
        evolve = !evolve;
        evolveSkillButton?.SetMaterial(evolve ? shiningMaterial : null);
    }

    public void ShowEvolveSkillInfo() {
        infoPrompt.SetSkill(new Skill(){ 
            name = "专属动作" + (evolve ? "（正在使用）" : string.Empty),
            critical = 5,
            accuracy = 100,
            rawDescription = "手动点击使用当前精灵的专属动作[ENDL][ffbb33]【神迹觉醒】[-]全队共用1次[ENDL][ffbb33]【暴走】[-]消耗道具但不消耗次数"
        });
    }

}

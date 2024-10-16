using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetChangeView : BattleBaseView
{
    private BattlePet[] petBag;
    private int cursor;
    [SerializeField] private BattlePetChangeBlockView[] changeBlockViews = new BattlePetChangeBlockView[6];

    public void SetPetBag(BattlePet[] petBag) {
        Array.Resize(ref petBag, 6);
        this.petBag = petBag;

        SetChangeBlocks(petBag);
    }

    private void SetChangeBlocks(BattlePet[] petBag) {
        for (int i = 0; i < 6; i++) {
            changeBlockViews[i].SetPet(petBag[i]);        
        }
    }

    public void SetChangeBlockChosen(int index)
    {
        if (!index.IsInRange(0, changeBlockViews.Length))
           return;

        cursor = index;
        for(int i = 0; i < 6; i++) {
            changeBlockViews[i].SetChosen(i == index);
        }
    }

    public void SetSkillSelectMode(bool isSkillSelectMode) {
        if (!isSkillSelectMode)
            return;

        var skill = battle.currentState.myUnit.skill;
        var isSelectDead = skill.effects.Exists(x => x.targetType.Contains("dead"));
        var isSelectOther = skill.effects.Exists(x => x.targetType.Contains("other"));

        for (int i = 0; i < 6; i++) {    
            var isTarget = (petBag[i] != null) && (isSelectDead ^ (!petBag[i].isDead));
            var isCursor = isSelectOther && (cursor == i);
            var interactable = isTarget && (!isCursor);
            
            changeBlockViews[i].SetFightingTag(false);
            changeBlockViews[i].SetInteractable(interactable, !interactable);
        }
    }

    public void SelectPet(int index) {
        if (!index.IsInRange(0, changeBlockViews.Length))
            return;
        
        if (UI.isSkillSelectMode) {
            var skill = battle.currentState.myUnit.skill;
            foreach (var e in skill.effects.Where(x => x.IsSelect()))
                e.abilityOptionDict.Set("target_index", index.ToString());
            
            battle.SetSkill(skill, true);
            return;
        }
            
        battle.SetSkill(Skill.GetPetChangeSkill(cursor, index, battle.currentState.isAnyPetDead), true);
        SetChangeBlockChosen(index);
    }

    public void ShowPetChangeInfo(int index) {
        if (!index.IsInRange(0, petBag.Length))
            return;

        var pet = petBag[index];
        string header = "<size=4>\n</size><size=16><color=#52e5f9>  " + pet.name + "</color></size><size=6>\n\n</size>";
        string content = "<size=16>  Lv " + pet.level + "\n  HP " + pet.hp + " / " + pet.maxHp + "</size>";
        descriptionBox.SetBoxSize(new Vector2(170, 150));
        descriptionBox.SetBoxPosition(new Vector2(100 + index * 110, 109));
        descriptionBox.SetText(header + content);
    }

}

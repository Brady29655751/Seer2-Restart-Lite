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

    public void SelectPet(int index) {
        if (!index.IsInRange(0, changeBlockViews.Length))
            return;
        
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

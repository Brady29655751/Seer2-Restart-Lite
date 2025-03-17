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
    [SerializeField] private GameObject parallelIndicator;
    [SerializeField] private BattlePetChangeBlockView[] changeBlockViews = new BattlePetChangeBlockView[6];

    public override void Init() {
        parallelIndicator?.SetActive(battle.settings.parallelCount > 1);
    }

    public void SetPetBag(BattlePet[] petBag) {
        Array.Resize(ref petBag, 6);
        this.petBag = petBag;

        SetChangeBlocks(petBag);
    }

    private void SetChangeBlocks(BattlePet[] petBag) {
        for (int i = 0; i < 6; i++)
            changeBlockViews[i].SetPet(petBag[i]);     
    }

    public void SetChangeBlockChosen(int index, int parallelIndex = -1)
    {
        if (!index.IsInRange(0, changeBlockViews.Length))
           return;

        cursor = index;
        for(int i = 0; i < 6; i++)
            changeBlockViews[i].SetChosen(i == index);

        if (parallelIndex.IsInRange(0, changeBlockViews.Length))
            changeBlockViews[5 - parallelIndex].SetChosen(true);
    }

    public void SetChangeBlockInteractable(int index, bool interactable) {
        if (!index.IsInRange(0, changeBlockViews.Length))
            return;

        changeBlockViews[index].SetInteractable(interactable, !interactable);
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
            var isParallel = (battle.settings.parallelCount <= 1) || (i < battle.settings.parallelCount);
            var interactable = isTarget && (!isCursor) && isParallel;
            
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

        if (battle.settings.parallelCount > 1) {
            bool isMe = index < (petBag.Length / 2);
            int targetIndex = isMe ? index : (petBag.Length - index - 1);
            var unit = isMe ? battle.currentState.myUnit : battle.currentState.opUnit;

            unit.petSystem.cursor = targetIndex;
            UI.SetState(null, battle.currentState);
            UI.ProcessQuery(true);
            return;
        }
        
        battle.SetSkill(Skill.GetPetChangeSkill(cursor, index, battle.currentState.isAnyPetDead), true);
        SetChangeBlockChosen(index);
    }

    public void ShowPetChangeInfo(int index) {
        if (!index.IsInRange(0, petBag.Length))
            return;

        var pet = petBag[index];
        var opPet = UI.currentState.opUnit.pet;
        
        string header = "<size=4>\n</size><size=16><color=#52e5f9>  " + pet.name + "</color></size><size=6>\n\n</size>";
        string content = "<size=16>HP " + pet.hp + " / " + pet.maxHp + "</size><size=4>\n\n</size>";

        float attack = PetElementSystem.GetElementRelation(pet.battleElementId, opPet);
        float subAttack = PetElementSystem.GetElementRelation(pet.subBattleElementId, opPet);
        float defense = PetElementSystem.GetElementRelation(opPet.battleElementId, pet);
        float subDefense = PetElementSystem.GetElementRelation(opPet.subBattleElementId, pet);

        var attackElement = pet.battleElement;
        var subAttackElement = pet.subBattleElement;
        var defenseElement = opPet.battleElement;
        var subDefenseElement = opPet.subBattleElement;

        var maxLength = Mathf.Max(attackElement.GetElementName().Length, defenseElement.GetElementName().Length,
            (subAttackElement == Element.普通) ? 0 : subAttackElement.GetElementName().Length,
            (subDefenseElement == Element.普通) ? 0 : subDefenseElement.GetElementName().Length);
        
        if (battle.settings.parallelCount <= 1) 
        {
            content += "我方 <b><color=#ffbb33>" + attackElement.GetElementName(maxLength) + "系</color></b> <color=#" 
                + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(attack)) + ">" 
                + PetElementSystem.GetElementRelationNote(attack) + " >> " + "</color>对方";

            if (subAttackElement != Element.普通)
                content += "\n我方 <b><color=#ffbb33>" + subAttackElement.GetElementName(maxLength) + "系</color></b> <color=#" 
                    + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(subAttack)) + ">" 
                    + PetElementSystem.GetElementRelationNote(subAttack) + " >> " + "</color>对方";

            content += "\n敌方 <b><color=#ffbb33>" + defenseElement.GetElementName(maxLength) + "系</color></b> <color=#" 
                + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(defense)) + ">" 
                + PetElementSystem.GetElementRelationNote(defense) + " << " + "</color>我方";

            if (subDefenseElement != Element.普通)
                content += "\n敌方 <b><color=#ffbb33>" + subDefenseElement.GetElementName(maxLength) + "系</color></b> <color=#" 
                    + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(subDefense)) + ">" 
                    + PetElementSystem.GetElementRelationNote(subDefense) + " << " + "</color>我方";
        }

        descriptionBox.SetBoxSize(new Vector2(190, 160));
        descriptionBox.SetBoxPosition(new Vector2(100 + index * 110, 109));
        descriptionBox.SetText(header + content);
    }

}

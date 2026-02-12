using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetChangeView : BattleBaseView
{
    private BattlePet[] petBag;
    private bool isSkillSelectMode = false;
    private int cursor, page = 0;
    private int lastPage => (petBag.Length - 1) / 6;
    [SerializeField] private GameObject parallelIndicator;
    [SerializeField] private BattlePetChangeBlockView[] changeBlockViews = new BattlePetChangeBlockView[6];
    [SerializeField] private PageView pageView;

    public override void Init() {
        parallelIndicator?.SetActive(battle.settings.parallelCount > 1);
    }

    public void SetPetBag(BattlePet[] petBag)
    {
        Array.Resize(ref petBag, (petBag.Length + 5) / 6 * 6);
        this.petBag = petBag;
    }

    private void SetPage(int page)
    {
        this.page = Mathf.Clamp(page, 0, lastPage);
        pageView?.gameObject.SetActive(lastPage > 0);
        pageView?.SetPage(page, lastPage);

        SetPetChangeBlocks(petBag);
        SetPetChangeBlockChosen();
        SetSkillSelectMode(isSkillSelectMode);
    }

    public void PrevPage()
    {
        SetPage(page - 1);
    }

    public void NextPage()
    {
        SetPage(page + 1);
    }

    private void SetPetChangeBlocks(BattlePet[] petBag)
    {
        for (int i = 0; i < 6; i++)
            changeBlockViews[i].SetPet(petBag[page * 6 + i]);
    }

    private void SetPetChangeBlockChosen()
    {
        for(int i = 0; i < 6; i++)
            changeBlockViews[i].SetChosen((page * 6 + i) == cursor);
    }

    public void SetChangeBlockInteractable(int index, bool interactable) {
        if (!index.IsInRange(0, changeBlockViews.Length))
            return;

        changeBlockViews[index].SetInteractable(interactable, !interactable);
    }

    public void SetSkillSelectMode(bool isSkillSelectMode) {
        this.isSkillSelectMode = isSkillSelectMode;
        if (!isSkillSelectMode)
            return;
        
        /*
        if (battle.settings.isPVP)
        {
            var skill = battle.currentState.myUnit.skill;
            var isSelectDead = skill.effects.Exists(x => x.targetType.Contains("dead"));
            var isSelectOther = skill.effects.Exists(x => x.targetType.Contains("other"));
            for (int i = 0; i < 6; i++) {  
                var index = page * 6 + i;  
                var isTarget = (petBag[index] != null) && (isSelectDead ^ (!petBag[index].isDead));
                var isCursor = isSelectOther && (cursor == index);
                var isParallel = (battle.settings.parallelCount <= 1) || (i < battle.settings.parallelCount);
                var interactable = isTarget && (!isCursor) && isParallel;

                changeBlockViews[i].SetFightingTag(false);
                changeBlockViews[i].SetInteractable(interactable, !interactable);
            }   
            return;
        }
        */

        var selectableTargets = battle.currentState.myUnit.skill.GetSelectableTarget(petBag, cursor, battle.settings.parallelCount);
        for (int i = 0; i < 6; i++)
        {
            changeBlockViews[i].SetFightingTag(false);
            changeBlockViews[i].SetInteractable(selectableTargets[page * 6 + i], !selectableTargets[page * 6 + i]);
        }
    }

    public void SetCursor(int index, int parallelIndex = -1)
    {
        if (index < 0)
           return;
    
        cursor = index;
        SetPage(cursor / 6);
    
        if (parallelIndex.IsInRange(0, changeBlockViews.Length))
            changeBlockViews[5 - parallelIndex].SetChosen(true);
    }

    public void SelectPet(int index)
    {
        if (!index.IsInRange(0, changeBlockViews.Length))
            return;

        SetDescriptionBoxActive(false);

        index += page * 6;
        if (UI.isSkillSelectMode)
        {
            var skill = battle.currentState.myUnit.skill;
            foreach (var e in skill.effects.Where(x => x.IsSelect()))
                e.abilityOptionDict.Set("target_index", index.ToString());

            battle.SetSkill(skill, true);
            return;
        }

        if (battle.settings.parallelCount > 1)
        {
            bool isMe = index < (petBag.Length / 2);
            int targetIndex = isMe ? index : (petBag.Length - index - 1);
            var unit = isMe ? battle.currentState.myUnit : battle.currentState.opUnit;

            unit.petSystem.cursor = targetIndex;
            UI.SetState(null, battle.currentState);
            UI.ProcessQuery(true);
            return;
        }

        battle.SetSkill(Skill.GetPetChangeSkill(cursor, index, battle.currentState.isAnyPetDead), true);
        SetCursor(index);
    }

    public void ShowPetChangeInfo(int index)
    {
        if (index < 0)
        {
            SetInfoPromptContent(GetPetChangeInfo(cursor % 6, false));
            infoPrompt.SetPositionOffset(new Vector2(infoPrompt.zeroFixPos.x, -infoPrompt.size.y + 24));
            return;
        }
            
        ShowPetChangeInfo(index, GetPetChangeInfo(index, false));
    }

    public void ShowOpPetChangeInfo(int index)
    {
        ShowPetChangeInfo(index, GetPetChangeInfo(index, true));
    }

    public string GetPetChangeInfo(int index, bool isOp)
    {
        if (!index.IsInRange(0, petBag.Length))
            return null;

        var cursor = page * 6 + index;
        var pet = isOp ? UI.currentState.myUnit.pet : petBag[cursor];
        var opPet = isOp ? petBag[cursor] : UI.currentState.opUnit.pet;
        var infoPet = isOp ? opPet : pet;

        string header = "<size=4>\n</size><size=16><color=#52e5f9>  " + infoPet.name + "</color></size><size=6>\n\n</size>";
        string content = "<size=16>HP " + infoPet.hp + " / " + infoPet.maxHp + "</size><size=4>\n\n</size>";

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
                + PetElementSystem.GetElementRelationNote(attack) + " >> " + "</color>对方  <color=#ffbb33>" + attack.ToString() + "</color>";

            if (subAttackElement != Element.普通)
                content += "\n我方 <b><color=#ffbb33>" + subAttackElement.GetElementName(maxLength) + "系</color></b> <color=#"
                    + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(subAttack)) + ">"
                    + PetElementSystem.GetElementRelationNote(subAttack) + " >> " + "</color>对方  <color=#ffbb33>" + subAttack.ToString() + "</color>";

            content += "\n敌方 <b><color=#ffbb33>" + defenseElement.GetElementName(maxLength) + "系</color></b> <color=#"
                + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(defense)) + ">"
                + PetElementSystem.GetElementRelationNote(defense) + " << " + "</color>我方  <color=#ffbb33>" + defense.ToString() + "</color>";

            if (subDefenseElement != Element.普通)
                content += "\n敌方 <b><color=#ffbb33>" + subDefenseElement.GetElementName(maxLength) + "系</color></b> <color=#"
                    + ColorUtility.ToHtmlStringRGB(PetElementSystem.GetElementRelationColor(subDefense)) + ">"
                    + PetElementSystem.GetElementRelationNote(subDefense) + " << " + "</color>我方  <color=#ffbb33>" + subDefense.ToString() + "</color>";
        }

        return header + content;
    }

    public void ShowPetChangeInfo(int index, string text)
    {        
        descriptionBox.SetBoxSize(new Vector2(220, 170));
        descriptionBox.SetBoxPosition(new Vector2(100 + index * 110, 109));
        descriptionBox.SetText(text);
    }

}

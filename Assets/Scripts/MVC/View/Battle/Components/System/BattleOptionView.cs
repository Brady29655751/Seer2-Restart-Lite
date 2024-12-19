using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleOptionView : BattleBaseView
{
    [SerializeField] private BattleOptionSelectView optionSelectView;
    [SerializeField] private BattlePetSkillView skillView;
    [SerializeField] private BattlePetItemController captureController;
    [SerializeField] private BattlePetItemController itemController;
    [SerializeField] private BattlePetChangeView changeView;
    [SerializeField] private Hintbox escapeView;
    [SerializeField] private BattleAutoView autoView;

    private Module[] options => new Module[5] { skillView, changeView, captureController, itemController, escapeView };

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < options.Length; i++) {
            options[i]?.gameObject.SetActive(true);
            options[i]?.gameObject.SetActive(i == 0);
        }

        autoView?.gameObject.SetActive(battle.settings.isAutoOK);
    }

    public void Select(int index) {
        optionSelectView.Select(index);
        for (int i = 0; i < options.Length; i++) {
            options[i]?.gameObject.SetActive(i == index);
        }
    }

    public void SetOptionActive(int index, bool active) {
        optionSelectView.SetInteractable(index, active);
    }

    public void SetBottomBarInteractable(bool interactable) {
        if (!interactable)
            Select(0);
        
        optionSelectView.SetInteractableAll(interactable);
        skillView.SetInteractable(interactable);
    }

    public void SetSkillSelectMode(bool isSkillSelectMode) {
        changeView.SetSkillSelectMode(isSkillSelectMode);
    }

    public void StopAutoBattle() {
        autoView?.OnToggleAutoBattle(false);
    }

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;
        
        int parallelCount = currentState.settings.parallelCount;
        Unit myUnit = currentState.myUnit;
        Unit opUnit = currentState.opUnit;
        var myBag = myUnit.petSystem.petBag;
        var opBag = opUnit.petSystem.petBag;
        var petBag = (parallelCount > 1) ? myBag.Take(parallelCount).Concat(Enumerable.Repeat(default(BattlePet), 6 - 2 * parallelCount))
            .Concat(opBag.Take(parallelCount).Reverse()).ToArray() : myBag;
        var parallelCursor = (parallelCount > 1) ? opUnit.petSystem.cursor : -1;
        var itemBag = (currentState.settings.mode == BattleMode.YiTeRogue) ? YiTeRogueData.instance.itemBag : Player.instance.gameData.itemBag;

        skillView.SetPet(myUnit.pet);
        changeView.SetPetBag(petBag);
        changeView.SetChangeBlockChosen(myUnit.petSystem.cursor, parallelCursor);
        captureController.SetItemBag(itemBag);
        itemController.SetItemBag(itemBag);

        if (parallelCount > 1)
            for (int i = 0; i < parallelCount; i++)
                if (myUnit.parallelSkillSystems.Get(i)?.skill != null)
                    changeView.SetChangeBlockInteractable(i, false);
    }

}


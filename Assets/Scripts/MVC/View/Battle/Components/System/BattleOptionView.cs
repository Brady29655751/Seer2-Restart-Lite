using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOptionView : BattleBaseView
{
    [SerializeField] private BattleOptionSelectView optionSelectView;
    [SerializeField] private BattlePetSkillView skillView;
    [SerializeField] private BattlePetItemController captureController;
    [SerializeField] private BattlePetItemController itemController;
    [SerializeField] private BattlePetChangeView changeView;
    [SerializeField] private Hintbox escapeView;

    private Module[] options => new Module[5] { skillView, changeView, captureController, itemController, escapeView };

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < options.Length; i++) {
            options[i]?.gameObject.SetActive(true);
            options[i]?.gameObject.SetActive(i == 0);
        }
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
        if (!interactable) {
            Select(0);
        }
        optionSelectView.SetInteractableAll(interactable);
        skillView.SetInteractable(interactable);
    }

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;
        
        Unit currentUnit = currentState.myUnit;
        skillView.SetPet(currentUnit.pet);
        changeView.SetPetBag(currentUnit.petSystem.petBag);
        changeView.SetChangeBlockChosen(currentUnit.petSystem.cursor);
        captureController.SetItemBag(Player.instance.gameData.itemBag);
        itemController.SetItemBag(Player.instance.gameData.itemBag);
    }

}


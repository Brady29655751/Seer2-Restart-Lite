using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleOptionView : BattleBaseView
{
    [SerializeField] private ExtendButton recordExtendButton;
    [SerializeField] private GameObject recordPanel;
    [SerializeField] private IText recordText;
    [SerializeField] private BattleOptionSelectView optionSelectView;
    [SerializeField] private BattlePetSkillView skillView;
    [SerializeField] private BattlePetItemController captureController;
    [SerializeField] private BattlePetItemController itemController;
    [SerializeField] private BattlePetChangeView changeView, opChangeView;
    [SerializeField] private Hintbox escapeView;
    [SerializeField] private BattleAutoView autoView;

    private Module[] options => new Module[] { skillView, changeView, captureController, itemController, escapeView, opChangeView };

    private ScrollRect recordScrollRect;
    private bool isRecordPanelActive => (recordPanel != null) && recordPanel.activeSelf;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < options.Length; i++) {
            options[i]?.gameObject.SetActive(true);
            options[i]?.gameObject.SetActive(i == 0);
        }

        autoView?.gameObject.SetActive(battle.settings.isAutoOK);

        if (isRecordPanelActive)
        {
            recordScrollRect = recordPanel.GetComponentInChildren<ScrollRect>();
            ToggleRecordPanel();   
        }
    }

    public void ToggleRecordPanel()
    {
        var isOn = isRecordPanelActive;
        if (recordExtendButton?.image?.rectTransform != null)
        {
            var pos = recordExtendButton.image.rectTransform.anchoredPosition;
            recordExtendButton.image.rectTransform.anchoredPosition = new Vector2(pos.x, pos.y + (isOn ? -1 : 1) * 180);
            recordExtendButton.SetMode(isOn);
        }

        if (recordPanel != null)
        {
            recordScrollRect.verticalNormalizedPosition = 0;
            recordPanel.SetActive(!isOn);
        }
    }

    public void Select(int index) {
        if ((options.ElementAtOrDefault(index) == opChangeView) && (!battle.settings.isReveal))
        {
            Hintbox.OpenHintboxWithContent("当前模式下，对手阵容不公开！", 16);
            return;
        }

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

        if (recordPanel != null)
        {
            recordText.SetText(currentState.reports.ConcatToString("\n"));
            recordScrollRect.verticalNormalizedPosition = 0;
        }
        skillView.SetPetSystem(myUnit.petSystem);
        changeView.SetPetBag(petBag);
        changeView.SetCursor(myUnit.petSystem.cursor, parallelCursor);
        opChangeView.SetPetBag(opBag);
        opChangeView.SetCursor(opUnit.petSystem.cursor, -1);
        captureController.SetItemBag(itemBag);
        itemController.SetItemBag(itemBag);

        if (parallelCount > 1)
            for (int i = 0; i < parallelCount; i++)
                if (myUnit.parallelSkillSystems.Get(i)?.skill != null)
                    changeView.SetChangeBlockInteractable(i, false);
    }

}


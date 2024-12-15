using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YiTeRoguePanel : Panel
{
    [SerializeField] private YiTeRogueView rogueView;
    [SerializeField] private PetBagPanel petBagPanel;

    public YiTeRogueData rogueData => YiTeRogueData.instance;
    public const int ROGUE_NPC_ID = 50001;
    
    public override void Init() {
        Player.instance.currentNpcId = ROGUE_NPC_ID;
        if ((rogueData == null) || (rogueData.difficulty == YiTeRogueMode.None) || rogueData.isEnd)
            YiTeRogueData.CreateRogue(YiTeRogueMode.Test);
        
        rogueView.SetMap();
    }

    public void OpenChoicePanel(YiTeRogueEvent rogueEvent, bool withStep = true) {
        rogueView.OpenChoicePanel(rogueEvent, withStep);
    }

    public void CloseChoicePanel() {
        rogueView.CloseChoicePanel();
        if (!YiTeRogueData.instance.isEnd)
            return;

        ClosePanel();
    }

    public void OpenPetBag() {
        if (rogueData.petBag.All(x => x == null)) {
            Hintbox.OpenHintboxWithContent("背包中还没有任何精灵", 16);
            return;
        }
        petBagPanel?.SetActive(true);
        petBagPanel?.SetPetBag(rogueData.petBag);
        petBagPanel?.SetItemBag(rogueData.itemBag);
    }

    public void ClosePetBag() {
        petBagPanel?.SetActive(false);
        Player.instance.gameData.yiteRogueData.petBag = petBagPanel.petBag;
        SaveSystem.SaveData();
    }

    public void CheckCurrentBuffs() {
        rogueView.ToggleBuffPanel();
    }
    
}

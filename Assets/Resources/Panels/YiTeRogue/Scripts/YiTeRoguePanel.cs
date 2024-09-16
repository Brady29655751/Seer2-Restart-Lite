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
    
    

    public override void Init() {
        //if (List.IsNullOrEmpty(rogueData.eventMap))
            rogueData.CreateRogue();
        
        rogueView.SetMap();
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

    }
    
}

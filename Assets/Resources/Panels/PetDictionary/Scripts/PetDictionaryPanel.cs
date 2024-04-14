using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictionaryPanel : Panel
{
    private List<Pet> petStorage => Player.instance.gameData.petStorage;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetDictInfoController infoController;
    

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    private void InitSelectSubscriptions() {
        selectController.onSelectPetEvent += demoController.SetPet;
        selectController.onSelectPetEvent += infoController.SetPet;
    }

}

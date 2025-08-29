using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictionaryPanel : Panel
{
    private List<Pet> petStorage => Player.instance.gameData.petStorage;
    [SerializeField] private PetDictionaryController dictController;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetDictInfoController infoController;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        SelectMode(PetDictionaryMode.All);
    }

    private void InitSelectSubscriptions()
    {
        selectController.onSelectPetEvent += demoController.SetPet;
        selectController.onSelectPetEvent += infoController.SetPet;
    }

    public void SetEditPetCallback(Action<PetInfo> callback) {
        infoController.SetEditPetCallback(callback);
    }

    public void SelectMode(PetDictionaryMode mode) {
        dictController.SelectMode((int)mode);
        infoController.SelectMode(dictController.GetMode());
    }

    public void SetStorage(List<Pet> storage) {
        dictController.SetPetStorage(storage);
    }

}

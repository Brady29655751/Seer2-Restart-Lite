using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSwitchController : Module
{
    [SerializeField] private PetSwitchModel switchModel;
    [SerializeField] private PetSwitchView switchView;
    [SerializeField] private PetSelectController selectController;
    
    public event Action<Pet> onSwitchSuccessEvent;

    public override void Init()
    {
        base.Init();
        selectController.onSelectPetEvent += switchModel.SetPet;
        selectController.onSelectPetEvent += switchView.SetPet;
        SetStorage(GameManager.versionData.petData.petAllWithMod);
    }

    public void SetStorage(List<Pet> storage) {
        selectController.SetStorage(storage);
        selectController.Select(0);
    }

    public void OnConfirmSwitch() {
        switchView.OnConfirmSwitch();
        onSwitchSuccessEvent?.Invoke(new Pet(switchModel.currentPet));
    }
}

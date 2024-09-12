using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStorageController : Module
{
    [SerializeField] private PetStorageModel buttonModel;
    [SerializeField] private PetStorageView buttonView;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    private void InitSelectSubscriptions() {
        selectController.onSetStorageEvent += buttonView.OnSetStorage;
        selectController.onSelectPetEvent += buttonView.OnSelect;
    }   

    public void SetPetStorage() {
        int page = buttonModel.storageSelectRefreshPage;
        int cursor = buttonModel.storageSelectCursor;
        var petStorage = Player.instance.gameData.petStorage;
        selectController.SetStorage(petStorage.ToList(), page);
        selectController.Select(cursor);
    }

    public void OnPetTake() {
        demoController?.SetPetAnimationActive(false);
        if (buttonModel.OnPetTake()) {
            OnAfterPetTake();
            return;
        }
        Action<Pet> callback = (pet) => {
            buttonModel.SetPetTake(pet);
            OnAfterPetTake();
        };
        buttonView.OnPetExchange(callback, () => demoController?.SetPetAnimationActive(true));
    }

    private void OnAfterPetTake() {
        SetPetStorage();
        buttonView.OnAfterPetTake();
    }

    public void OnPetRelease() {
        buttonView.OnPetRelease(SetPetRelease);
    }

    private void SetPetRelease() {
        buttonModel.SetPetRelease();
        SetPetStorage();
    }

}

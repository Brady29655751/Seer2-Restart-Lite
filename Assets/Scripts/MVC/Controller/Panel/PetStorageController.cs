using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStorageController : Module
{
    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetStorageModel buttonModel;
    [SerializeField] private PetStorageView buttonView;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        buttonModel.SetMode(mode);
    }

    private void InitSelectSubscriptions() {
        selectController.onSetStorageEvent += buttonView.OnSetStorage;
        selectController.onSelectPetEvent += buttonView.OnSelect;
    }   

    public void SetPetStorage() {
        int page = (mode == PetBagMode.PVP) ? buttonModel.storageSelectPage : buttonModel.storageSelectRefreshPage;
        int cursor = buttonModel.storageSelectCursor;
        var petStorage = buttonModel.petStorage;
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
        buttonView.OnPetExchange(buttonModel.petBag, callback, () => demoController?.SetPetAnimationActive(true));
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

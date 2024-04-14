using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagController : Module
{
    private Pet[] petBag => GetPetBag();
    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetBagModel buttonModel;
    [SerializeField] private PetBagView buttonView;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetPersonalityController personalityController;

    protected override void Awake()
    {
        base.Awake();
        InitSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        buttonModel.SetMode(mode);
    }

    private void InitSubscriptions() {
        selectController.onSelectIndexEvent += buttonView.OnSelect;
        
        if (mode == PetBagMode.Normal)
            personalityController.onSelectPersonalityEvent += ToBestPet;
    }

    private Pet[] GetPetBag() {
        return mode switch {
            PetBagMode.Normal => Player.instance.gameData.petBag,
            PetBagMode.PVP => selectController.GetPetSelections(),
            _ => Player.instance.gameData.petBag,
        };
    }

    public void RefreshPetBag() {
        selectController.SetStorage(petBag.ToList());
        selectController.Select(0);
    }

    public void SetPetFirst() {
        buttonModel.SetPetFirst();
        RefreshPetBag();
    }

    public void SetPetHeal() {
        buttonModel.SetPetHeal();
        buttonView.OnSetPetHeal();
        selectController.RefreshView();
    }

    public void SetPetTrain() {
        personalityController.SetActive(true);
    }

    public void ToBestPet(Personality personality) {
        personalityController.SetActive(false);
        if (buttonModel.SetPetTrain() == null)
            return;

        buttonView.OnSetPetTrain();
        RefreshPetBag();
    }

    public void SetPetHome() {
        if (!buttonModel.SetPetHome()) {
            buttonView.OnSetPetHomeFailed();
        }
        RefreshPetBag();
    }

    public void OpenPetStoragePanel() {
        PetStoragePanel storagePanel = Panel.OpenPanel<PetStoragePanel>();
        storagePanel.onCloseEvent += RefreshPetBag;
    }
    
}

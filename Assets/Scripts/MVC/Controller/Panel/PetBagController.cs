using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagController : Module
{
    private Pet[] petBag => GetPetBag();
    private bool isDemoMode = true;

    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetBagModel buttonModel;
    [SerializeField] private PetBagView buttonView;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetPersonalityController personalityController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetFeatureController featureController;
    [SerializeField] private PetSkinController skinController;

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

    public void SetPetDrop(int dropIndex, RectTransform rectTransform = null) {
        buttonModel.SetPetDrop(dropIndex);
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

    public void SetPetAnim() {
        demoController?.TogglePetAnimationMode();
    }

    public void SetPetTrain() {
        isDemoMode = !isDemoMode;
        demoController.gameObject.SetActive(isDemoMode);
        featureController.gameObject.SetActive(!isDemoMode);
    }

    public void ToBestPet(Personality personality) {
        personalityController.onSelectPersonalityEvent -= ToBestPet;
        personalityController.SetActive(false);
        if (buttonModel.SetPetTrain() == null)
            return;

        buttonView.OnSetPetTrain();
        RefreshPetBag();
    }

    public void SetPetPersonality() {
        personalityController.onSelectPersonalityEvent += ToPersonalityPet;
        personalityController.SetActive(true);
    }

    public void ToPersonalityPet(Personality personality) {
        personalityController.onSelectPersonalityEvent -= ToPersonalityPet;
        personalityController.SetActive(false);
        
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
    
    public void TogglePetItemPanel() {
        buttonView.TogglePetItemPanel();
    }
}

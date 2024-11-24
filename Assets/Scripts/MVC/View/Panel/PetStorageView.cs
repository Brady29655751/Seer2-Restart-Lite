using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStorageView : Module
{
    [SerializeField] private IButton takeButton;
    [SerializeField] private IButton releaseButton;

    public void OnSetStorage() {
        SetInteractable(false);
    }

    public void OnSelect(Pet pet) {
        SetInteractable(pet != null);
    }

    public void SetInteractable(bool interactable) {
        takeButton.SetInteractable(interactable);
        releaseButton.SetInteractable(interactable);
    }

    public void OnAfterPetTake() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("携带成功", 16, FontOption.Arial);
    }

    public void OnPetExchange(Pet[] petBag, Action<Pet> onExchangeCallback = null, Action onFailCallback = null) {
        PetSelectHintbox hintbox = Hintbox.OpenHintbox<PetSelectHintbox>();
        hintbox.SetStorage(petBag.ToList());
        hintbox.SetOptionNum(2);
        hintbox.SetTitle("请选择要交换的精灵");
        hintbox.SetConfirmSelectCallback(onExchangeCallback);
        hintbox.SetOptionCallback(onFailCallback, false);
    }

    public void OnPetRelease(Action confirmCallback = null) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(2);
        hintbox.SetTitle("放生");
        hintbox.SetContent("确定要放生吗？\n" + "此动作无法复原！", 16, FontOption.Arial);
        hintbox.SetOptionCallback(confirmCallback);
    }
}

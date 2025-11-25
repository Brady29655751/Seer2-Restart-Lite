using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PetSkinController : Module
{
    private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetSkinModel skinModel;
    [SerializeField] private PetSkinView skinView;
    [SerializeField] private PageView pageView;

    public event Action onSetSkinSuccessEvent;

    public void SetMode(PetBagMode mode) {
        this.mode = mode;
    }

    public void SetPet(Pet pet) {
        skinModel.SetPet(pet);
        OnSkinSetPage();
    }

    public void Select(int index) {
        skinModel.Select(index);
        skinView.Select(index);
        skinView.SetPetAnimation(skinModel.currentSkinId);
    }

    public void OnSkinConfirm() {
        skinModel.SetSkin(mode == PetBagMode.Normal);
        skinView.OnSkinConfirm();
        onSetSkinSuccessEvent?.Invoke();
    }

    public void OnSkinSetPage() {
        skinView.SetSkins(skinModel.selections.ToList());
        pageView?.SetPage(skinModel.page, skinModel.lastPage);
        Select(0);
    }

    public void OnSkinPrevPage() {
        skinModel.PrevPage();
        OnSkinSetPage();
    }

    public void OnSkinNextPage() {
        skinModel.NextPage();
        OnSkinSetPage();
    }
}

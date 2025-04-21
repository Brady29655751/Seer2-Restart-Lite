using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionView : Module
{
    [SerializeField] private PetSelectBlockView mainPetBlockView, subPetBlockView;
    [SerializeField] private List<PetItemBlockView> petItemBlockViews = new List<PetItemBlockView>();

    public override void Init()
    {
        base.Init();
        mainPetBlockView.SetPet(null);
        subPetBlockView.SetPet(null);
        petItemBlockViews.ForEach(x => x.SetItem(null));
    }

    public void SetMainPet(Pet pet) {
        mainPetBlockView.SetPet(pet);
    }

    public void SetSubPet(Pet pet) {
        subPetBlockView.SetPet(pet);
    }

    public void SetItem(List<Item> item) {
        for (int i = 0; i < petItemBlockViews.Count; i++)
            petItemBlockViews[i].SetItem((i < item.Count) ? item[i] : null);
    }
}

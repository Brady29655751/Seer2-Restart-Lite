using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PondPanel : Panel
{
    [SerializeField] private PetItemController pondItemController, insectItemController;

    private List<Animal> pond = new List<Animal>();
    private List<Animal> insectHouse = new List<Animal>();

    protected override void Awake()
    {
        base.Awake();
        pondItemController?.SetGif((x, i) => GetGif(pond, x, i));
        insectItemController?.SetGif((x, i) => GetGif(insectHouse, x, i));
    }

    private AnimInfo GetGif(List<Animal> animals, Item item, int index)
    {
        var animal = animals.Get(index);
        if (Animal.IsNullOrEmpty(animal) || (item == null))
            return null;

        return animal.GetGifInfo("front");
    }

    public override void SetPanelIdentifier(string id, string param)
    {
        switch (id)
        {
            default:
                base.SetPanelIdentifier(id, param);
                return;

            case "pond":
                if (param == "map")
                    SetPond(Player.instance.currentMap.entities.ponds?.Select(x => x.id));
                else
                    SetPond(param.ToIntList('/'));
                return;

            case "insect":
                if (param == "map")
                    SetInsectHouse(Player.instance.currentMap.entities.insects?.Select(x => x.id));
                else
                    SetInsectHouse(param.ToIntList('/'));
                return;
        }
    }

    public override void ClosePanel()
    {
        Player.SetSceneData("seed", 0);
        base.ClosePanel();
    }

    public void SetPond(IEnumerable<int> pondIds)
    {
        pondIds ??= new List<int>();
        RefreshPond(pondIds.Select(Animal.LoadData).Where(x => !Animal.IsNullOrEmpty(x)).ToList());
    }

    public void RefreshPond(List<Animal> fishList)
    {
        pondItemController.gameObject.SetActive(true);
        insectItemController?.gameObject.SetActive(false);

        fishList ??= new List<Animal>();
        pond = fishList;
        pondItemController.SetItemBag(fishList.Select(f => f.IsFollowing ? null : new Item(f.IsRiped ? f.id : f.ChildId, -1)).ToList());
    }

    public void ShowPondInfo(int index)
    {
        var fishIndex = pondItemController.GetCurrentPage() * 18 + index;
        var fish = pond.Get(fishIndex);
        infoPrompt.SetAnimal(fish);
    }

    public void OnPondItemClick(int index)
    {
        var fishIndex = pondItemController.GetCurrentPage() * 18 + index;
        if (Animal.IsNullOrEmpty(pond.Get(fishIndex)))
            return;

        var landId = Animal.IsNullOrEmpty(pond.Get(fishIndex)) ? 0 : pond.Get(fishIndex).landId;
        Animal.OnClick(landId, () => SetPond(pond.Select(x => Animal.IsNullOrEmpty(x) ? 0 : x.landId)));
    }



    public void SetInsectHouse(IEnumerable<int> insectIds)
    {
        insectIds ??= new List<int>();
        RefreshInsectHouse(insectIds.Select(Animal.LoadData).Where(x => !Animal.IsNullOrEmpty(x)).ToList());
    }

    public void RefreshInsectHouse(List<Animal> insectList)
    {
        pondItemController?.gameObject.SetActive(false);
        insectItemController.gameObject.SetActive(true);

        insectList ??= new List<Animal>();
        insectHouse = insectList;
        insectItemController.SetItemBag(insectList.Select(f => f.IsFollowing ? null : new Item(f.IsRiped ? f.id : f.ChildId, -1)).ToList());
    }

    public void ShowInsectInfo(int index)
    {
        var insectIndex = insectItemController.GetCurrentPage() * 8 + index;
        var insect = insectHouse.Get(insectIndex);
        infoPrompt.SetAnimal(insect);
    }

    public void OnInsectItemClick(int index)
    {
        var insectIndex = insectItemController.GetCurrentPage() * 8 + index;
        if (Animal.IsNullOrEmpty(insectHouse.Get(insectIndex)))
            return;
        
        var landId = Animal.IsNullOrEmpty(insectHouse.Get(insectIndex)) ? 0 : insectHouse.Get(insectIndex).landId;
        Animal.OnClick(landId, () => SetInsectHouse(insectHouse.Select(x => Animal.IsNullOrEmpty(x) ? 0 : x.landId)));
    }
}

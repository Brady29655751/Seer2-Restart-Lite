using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlantController : Module
{
    [SerializeField] private PetItemController itemController;

    protected override void Awake()
    {
        base.Awake();
        itemController.onItemSelectEvent += OnSelectSeed;
        Player.SetSceneData("seed", 0);
    }

    public override void Init() {
        base.Init();
        itemController.SetMode(PetBagMode.Dictionary);
        itemController.SetItemBag(Item.normalPlantItemDatabase);
    }

    public void OnSelectSeed(Item seed) {
        Player.SetSceneData("seed", seed.id);
    }
}

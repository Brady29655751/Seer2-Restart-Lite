using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPlantController : Module
{
    [SerializeField] private PetItemController seedController;
    private ItemType cursor = ItemType.Seed;


    protected override void Awake()
    {
        base.Awake();
        seedController.onItemSelectEvent += OnSelectSeed;
        Player.SetSceneData("seed", 0);
    }

    public override void Init()
    {
        base.Init();
        seedController.SetMode(PetBagMode.Dictionary);
        Refresh();
    }

    public static List<Item> GetSeedDictionary(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Fertilizer => Item.FindAll(x => x.info.type == ItemType.Fertilizer),
            _ => Item.normalPlantItemDatabase,  
        };
    }

    public void Refresh()
    {
        seedController.SetItemBag(GetSeedDictionary(cursor));
    }

    public void SelectItemType(string type)
    {
        var itemType = type.ToItemType();
        if (cursor == itemType)
            return;

        cursor = itemType;
        Refresh();
    }

    public void OnSelectSeed(Item seed)
    {
        Player.SetSceneData("seed", seed.id);
    }
}

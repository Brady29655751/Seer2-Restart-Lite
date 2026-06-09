using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapAnimalController : Module
{
    [SerializeField] private PetItemController animalController;
    private ItemType cursor = ItemType.Animal;

    protected override void Awake()
    {
        base.Awake();
        animalController.onItemSelectEvent += OnSelectAnimal;
        Player.SetSceneData("seed", 0);
    }
    public override void Init()
    {
        base.Init();
        animalController.SetMode(PetBagMode.Dictionary);
        Refresh();
    }
    public static List<Item> GetAnimalDictionary(ItemType itemType)
    {
        var animalDict = new List<Item>();
        var animals = ItemInfo.database.Where(x => x.type == ItemType.Animal).Select(x => new Animal(x.id, 0));
        var childs = animals.Select(x => (x.AnimalInfo, Item.Find(x.ChildId))).Where(x => !Item.IsNullOrEmpty(x.Item2));

        return itemType switch
        {
            ItemType.Animal => childs.Select(x => new Item(x.Item1.id, x.Item2.num)).ToList(),
            ItemType.AnimalAction => ItemInfo.database.Where(x => x.type == ItemType.AnimalAction).Select(x => new Item(x.id, -1)).ToList(),
            _ => new List<Item>(),  
        };
    }
    public void Refresh()
    {
        animalController.SetItemBag(GetAnimalDictionary(cursor));
    }

    public void SelectItemType(string type)
    {
        var itemType = type.ToItemType();
        if (cursor == itemType)
            return;
        
        cursor = itemType;
        Refresh();
    }

    public void OnSelectAnimal(Item animal)
    {
        var landIds = Player.instance.currentMap?.entities?.animals?.Select(x => x.id);
        if (ListHelper.IsNullOrEmpty(landIds))
        {
            Hintbox.OpenHintboxWithContent("这里不适合养殖动物哦！", 16);
            return;
        }

        if (animal.info.type == ItemType.AnimalAction)
        {
            var seed = animal.id == 68_0000 ? 0 : animal.id;
            Player.SetSceneData("seed", seed);
            return;
        }

        var landType = animal.info.options.Get("landType", "land").ToLandType();
        switch (landType)
        {
            default:
                break;

            case Animal.LandType.Land:
                foreach (var landId in landIds)
                {
                    var oldAnimal = Animal.LoadData(landId);
                    if (Animal.IsNullOrEmpty(oldAnimal))
                    {
                        Animal.NewAnimal(landId, animal.id);
                        return;
                    }
                }
                Hintbox.OpenHintboxWithContent("你的牧场太拥挤了，无法再养更多的动物了！", 16);
                break;
        }

    }
}

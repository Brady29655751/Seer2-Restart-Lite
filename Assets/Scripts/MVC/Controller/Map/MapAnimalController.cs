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
        Animal.NewAnimal(101101, animal.id);
    }
}

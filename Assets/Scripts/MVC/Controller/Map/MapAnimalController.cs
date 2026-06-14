using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapAnimalController : Module
{
    [SerializeField] private PetItemController animalController;
    [SerializeField] private ItemType cursor = ItemType.Animal;

    protected override void Awake()
    {
        base.Awake();
        animalController.onItemSelectEvent += OnSelectAnimalChild;
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
        // var animalDict = new List<Item>();
        // var animals = ItemInfo.database.Where(x => x.type == ItemType.Animal).Select(x => new Animal(x.id, 0));
        // var childs = animals.Select(x => (x.AnimalInfo, Item.Find(x.ChildId))).Where(x => !Item.IsNullOrEmpty(x.Item2));

        return itemType switch
        {
            ItemType.Animal => Item.FindAll(x => x.info.type == ItemType.AnimalChild), // childs.Select(x => new Item(x.Item1.id, x.Item2.num)).ToList(),
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

    public void OnSelectAnimalChild(Item child)
    {
        if (child.info.type == ItemType.AnimalAction)
        {
            OnSelectAnimal(child);
            return;
        }

        var animal = Item.animalItemDatabase.FirstOrDefault(x => new Animal(x.id, 0).ChildId == child.id);
        if (animal == null)
            return;
        
        OnSelectAnimal(animal);
    }

    private void OnSelectAnimal(Item animal)
    {
        var landIds = Player.instance.currentMap?.entities?.animals?.Select(x => x.id);
        var pondIds = Player.instance.currentMap?.entities?.ponds?.Select(x => x.id);
        var treeIds = Player.instance.currentMap?.entities?.insects?.Select(x => x.id);
        var nestIds = Player.instance.currentMap?.entities?.nests?.Select(x => x.id);

        if (animal.info.type == ItemType.AnimalAction)
        {
            var seed = animal.id == 68_0000 ? 0 : animal.id;
            Player.SetSceneData("seed", seed);
            return;
        }

        var landType = new Animal(animal.id, 0).AnimalLandType;
        var homeIds = landType switch
        {
            Animal.LandType.Land => landIds,
            Animal.LandType.Water => pondIds,
            Animal.LandType.Insect => treeIds,
            Animal.LandType.Nest => nestIds,
            _ => new List<int>(),
        };
        var fullHint = landType switch
        {
            Animal.LandType.Land => "你的牧场太拥挤了，无法再养更多的陆生动物了！",
            Animal.LandType.Water => "你的池塘太拥挤了，无法再养更多的水生动物了！",
            Animal.LandType.Insect => "你的昆虫小屋太拥挤了，无法再养更多的昆虫了！",
            Animal.LandType.Nest => "你的鸟巢太拥挤了，无法再孵更多的蛋了！",
            _ => "该动物为未知种类，无法养殖",
        };

        if (ListHelper.IsNullOrEmpty(homeIds))
        {
            Hintbox.OpenHintboxWithContent("这里不适合养殖该动物哦！", 16);
            return;
        }

        foreach (var homeId in homeIds)
        {
            var oldAnimal = Animal.LoadData(homeId);
            if (Animal.IsNullOrEmpty(oldAnimal))
            {
                Animal.NewAnimal(homeId, animal.id);
                return;
            }
        }
        
        Hintbox.OpenHintboxWithContent(fullHint, 16);
    }
}

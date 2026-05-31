using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Animal
{
    public int id, landId;
    public DateTime ripeDate, lastProduceDate;
    public int productAll, productBorned;

    public string Name => AnimalInfo?.name;
    public int ChildId => int.Parse(AnimalInfo.options.Get("child", "0"));
    public int ProductId => int.Parse(AnimalInfo.options.Get("product", "0"));
    public int FeedId => int.Parse(ChildInfo.options.Get("feed", "0"));

    public ItemInfo AnimalInfo => Item.GetItemInfo(id);
    public ItemInfo ChildInfo => Item.GetItemInfo(ChildId);
    public ItemInfo ProductInfo => Item.GetItemInfo(ProductId);
    public ItemInfo FeedInfo => Item.GetItemInfo(FeedId);

    public TimeSpan RipeTime => TimeSpan.Parse(ChildInfo.options.Get("time", "00:00:00"));
    public TimeSpan LeftTime => IsRiped ? TimeSpan.Zero : (ripeDate - DateTime.Now);
    public DateTime LastProduceDate => (lastProduceDate < ripeDate) ? ripeDate : lastProduceDate;
    public DateTime BirthDate => ripeDate - RipeTime;

    public string ProductNumRange => AnimalInfo.options.Get("num", "1");
    public string ProduceEachRange => AnimalInfo.options.Get("product.each", "1");
    public int ProductLeft => productAll - productBorned;

    public LandType HomeType => (LandType)int.Parse(ChildInfo.options.Get("landType", "0"));
    public string Adjective => AnimalInfo.options.Get("adj", "普通的");
    public bool IsRiped => DateTime.Now >= ripeDate;

    public Sprite Icon => GetIcon();

    public static Animal LoadData(int landId)
    {
        var activity = Activity.Find("animal");
        var prefix = $"land[{landId}]";
        var animal = new Animal()
        {
            id = int.TryParse(activity.GetData($"{prefix}.animal", "none"), out var num) ? num : 0,
            landId = landId,
            ripeDate = DateTime.TryParse(activity.GetData($"{prefix}.date[ripe]", DateTime.MaxValue.ToString()), out var date) ? date : DateTime.MaxValue,
            lastProduceDate = DateTime.TryParse(activity.GetData($"{prefix}.date[produce]", DateTime.MinValue.ToString()), out date) ? date : DateTime.MinValue,
            productAll = int.TryParse(activity.GetData($"{prefix}.product[all]", "0"), out num) ? num : 0,
            productBorned = int.TryParse(activity.GetData($"{prefix}.product[born]", "0"), out num) ? num : 0,
        };
        return animal;
    }

    public static List<Animal> LoadAll()
    {
        var activity = Activity.Find("animal");
        var landIds = activity.data.Where(x => x.key.StartsWith("land") && x.key.EndsWith(".animal")).Select(x => int.Parse(x.key.TrimParentheses()));
        return landIds.Select(LoadData).Where(x => !Animal.IsNullOrEmpty(x)).ToList();
    }

    public static void SaveData(Animal animal, bool autoSave = true)
    {
        var activity = Activity.Find("animal");
        var landId = animal.landId;
        var prefix = $"land[{landId}]";
        
        activity.SetData($"{prefix}.animal", animal.id);
        activity.SetData($"{prefix}.date[ripe]", animal.ripeDate);
        activity.SetData($"{prefix}.date[produce]", animal.LastProduceDate);
        activity.SetData($"{prefix}.product[all]", animal.productAll);
        activity.SetData($"{prefix}.product[born]", animal.productBorned);

        if (autoSave)
            SaveSystem.SaveData();
    }

    public static bool NewAnimal(int landId, int animalId)
    {
        var oldAnimal = Animal.LoadData(landId);
        if (!Animal.IsNullOrEmpty(oldAnimal))
            return false;

        var newAnimal = new Animal(animalId, landId);
        if (Animal.IsNullOrEmpty(newAnimal))
        {
            MapManager.instance.SetAnimalPanelActive(true);
            return false;
        }

        if (newAnimal.AnimalInfo.type != ItemType.Animal)
            return false;
        
        var child = Item.Find(newAnimal.ChildId);
        if (Item.IsNullOrEmpty(child))
        {
            Hintbox.OpenHintboxWithContent($"你没有<color=#ffbb33>{newAnimal.ChildInfo.name}</color>！\n先去旁边的木屋购买吧！", 14);
            return false;
        }

        // Born the animal.
        Item.Remove(newAnimal.ChildId, 1);
        Animal.SaveData(newAnimal);
        MapManager.instance.RefreshAnimalPanel();
        return true;   
    }

    public static Animal Harvest(int landId)
    {
        var animal = Animal.LoadData(landId);
        var name = animal.Name;
        var specialEffect = animal.AnimalInfo.effects.Find(x => x.abilityOptionDict.ContainsKey("animal"));
        var harvest = new Item(animal.id, 1);

        Animal.SaveData(Animal.DefaultAnimal(landId), false);
        Item.Add(harvest);
        Item.OpenHintbox(harvest);

        return animal;
    }

    public static bool IsNullOrEmpty(Animal animal)
    {
        return (animal == null) || (animal.AnimalInfo == null);
    }

    public static Animal DefaultAnimal(int landId = 0)
    {
        return new Animal()
        {
            id = 0,
            landId = landId,
            ripeDate = DateTime.MaxValue,
            lastProduceDate = DateTime.MaxValue,
            productAll = 0,
            productBorned = 0,
        };
    }

    public Animal(){}

    public Animal(int id, int landId)
    {
        this.id = id;
        this.landId = landId;
        this.ripeDate = (id == 0) ? DateTime.MaxValue : (DateTime.Now + RipeTime);
        this.lastProduceDate = this.ripeDate;
        this.productAll = (int)Identifier.GetNumIdentifier(ProductNumRange);
        this.productBorned = 0;
    }

    /// <summary>
    /// Returns 0 ~ (totalStep - 1)
    /// </summary>
    public int GetGrowth(int totalStep)
    {
        var growth = 1 - LeftTime / RipeTime;
        int index = (int)(growth * (totalStep - 1));
        if (index >= (totalStep - 1))
            index = totalStep - 1;

        return index;
    }

    public Sprite GetIcon()
    {
        var overrideIcon = ChildInfo.effects.Find(x => x.abilityOptionDict.ContainsKey("icon"));
        if (overrideIcon != null)
        {
            var iconList = overrideIcon.abilityOptionDict.Get("icon").ToIntList('/');
            var iconId = iconList.Last();
            if (!IsRiped)
            {
                var growth = 1 - (ripeDate - DateTime.Now) / RipeTime;
                int index = (int)(growth * (iconList.Count - 1));
                if (index >= iconList.Count)
                    index = iconList.Count - 1;
                
                iconId = iconList[index];
            }
            return ResourceManager.instance.GetLocalAddressables<Sprite>($"Maps/animal/{iconId}/icon", ItemInfo.IsMod(iconId));
        }

        return IsRiped ? AnimalInfo.icon : ChildInfo.icon;
    }

    public string GetGifUrl(string type)
    {
        var overrideGif = ChildInfo?.effects.Find(x => x.abilityOptionDict.ContainsKey("gif"));
        if (overrideGif == null)
            return null;
        
        var gifList = overrideGif.abilityOptionDict.Get("gif").ToIntList('/');
        var growth = GetGrowth(gifList.Count);
        return $"Maps/animal/{gifList[growth]}/{type}";
    }

    public bool Feed()
    {
        var feeder = Item.Find(FeedId);
        if (Item.IsNullOrEmpty(feeder))
            return false;

        Item.Remove(FeedId, 1);
        return true;
    }

    public Item Produce(Action<Item> callback = null)
    {
        if (!IsRiped)
            return null;

        if (ProductLeft <= 0)
            return null;

        var num = (int)Identifier.GetNumIdentifier(ProduceEachRange);
        var item = new Item(ProductId, num);
        productBorned -= num;
        lastProduceDate = DateTime.Now;
        Animal.SaveData(this, false);
        callback?.Invoke(item);
        return item;
    }
}

public enum LandType
{
    Land = 0,
    Water = 1,
    Insect = 2,
    Egg = 3,
}

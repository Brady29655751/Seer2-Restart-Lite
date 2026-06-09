using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Plant
{
    public int id, landId;
    public DateTime ripeDate;
    public float productMult;

    public string Name => PlantInfo?.name;
    public int SeedId => int.Parse(PlantInfo.options.Get("seed", "0"));

    public ItemInfo PlantInfo => Item.GetItemInfo(id);
    public ItemInfo SeedInfo => Item.GetItemInfo(SeedId);

    public TimeSpan RipeTime => TimeSpan.Parse(SeedInfo.options.Get("time", "00:00:00"));
    public TimeSpan LeftTime => IsRiped ? TimeSpan.Zero : (ripeDate - DateTime.Now);
    public DateTime BirthDate => ripeDate - RipeTime;

    public string ProductNumRange => PlantInfo.options.Get("num", "1");
    public bool IsRiped => DateTime.Now >= ripeDate;


    public static Plant LoadData(int landId)
    {
        var activity = Activity.Find("farm");
        var prefix = $"land[{landId}]";
        var plant = new Plant()
        {
            id = int.TryParse(activity.GetData($"{prefix}.plant", "none"), out var num) ? num : 0,
            landId = landId,
            ripeDate = DateTime.TryParse(activity.GetData($"{prefix}.date[ripe]", DateTime.MaxValue.ToString()), out var date) ? date : DateTime.MaxValue,
            productMult = float.TryParse(activity.GetData($"{prefix}.produce", "1"), out var mult) ? mult : 1,
        };
        return plant;
    }

    public static void SaveData(Plant plant, bool autoSave = true)
    {
        var activity = Activity.Find("farm");
        var landId = plant.landId;
        var prefix = $"land[{landId}]";
        activity.SetData($"{prefix}.plant", plant.id);
        activity.SetData($"{prefix}.date[ripe]", plant.ripeDate);    
        activity.SetData($"{prefix}.produce", plant.productMult);

        if (autoSave)
            SaveSystem.SaveData();
    }

    public static bool IsNullOrEmpty(Plant plant)
    {
        return (plant == null) || (plant.PlantInfo == null);
    }

    public static Plant DefaultPlant(int landId = 0)
    {
        return new Plant()
        {
            id = 0,
            landId = landId,
            ripeDate = DateTime.MaxValue,
            productMult = 1,
        };
    }

    public static bool NewPlant(int landId, int plantId)
    {
        var oldPlant = Plant.LoadData(landId);
        if (!Plant.IsNullOrEmpty(oldPlant))
            return false;
        
        var newPlant = new Plant(plantId, landId);
        if (Plant.IsNullOrEmpty(newPlant))
        {
            MapManager.instance.SetPlantPanelActive(true);
            return false;
        }

        if (newPlant.PlantInfo.type != ItemType.Plant)
            return false;

        var seed = Item.Find(newPlant.SeedId);
        if (Item.IsNullOrEmpty(seed))
        {
            Hintbox.OpenHintboxWithContent($"你没有<color=#ffbb33>{newPlant.SeedInfo.name}</color>！\n先去旁边的木屋购买吧！", 14);
            return false;
        }

        // Plant the seed.
        Item.Remove(newPlant.SeedId, 1);
        Plant.SaveData(newPlant);
        MapManager.instance.RefreshPlantPanel(new Item(plantId, Item.Find(newPlant.SeedId)?.num ?? 0));
        return true;
    }

    public static bool Fertilize(int landId, int fertilizerId)
    {
        var plant = Plant.LoadData(landId);
        if (Plant.IsNullOrEmpty(plant) || (plant.IsRiped))
            return false;

        var fertilizerInfo = Item.GetItemInfo(fertilizerId);
        if (fertilizerInfo == null)
        {
            MapManager.instance.SetPlantPanelActive(true);
            return false;
        }

        if (fertilizerId == 61_0000)
        {
            SaveData(Plant.DefaultPlant(landId));
            return false;
        }

        if (fertilizerInfo.type != ItemType.Fertilizer)
            return false;

        var fertilizer = Item.Find(fertilizerId);
        if (Item.IsNullOrEmpty(fertilizer))
        {
            Hintbox.OpenHintboxWithContent($"你没有<color=#ffbb33>{fertilizerInfo.name}</color>！\n先去旁边的木屋购买吧！", 14);
            return false;   
        }

        if (fertilizerInfo.options.TryGet("time", out var speedup))
            plant.ripeDate -= TimeSpan.Parse(speedup);

        if (fertilizerInfo.options.TryGet("product.mult", out var produceUp))
            plant.productMult = Identifier.GetNumIdentifier(produceUp);

        Item.Remove(fertilizerId, 1);
        Plant.SaveData(plant);
        MapManager.instance.RefreshPlantPanel(new Item(fertilizerId, Item.Find(fertilizerId)?.num ?? 0));
        return true;
    }

    public static Plant Harvest(int landId)
    {
        var plant = Plant.LoadData(landId);
        var name = plant.Name;
        var prdouctMult = plant.productMult;
        var specialEffect = plant.PlantInfo.effects.Find(x => x.abilityOptionDict.ContainsKey("plant"));
        var isSpecialSuccess = (specialEffect != null) && specialEffect.Condition(Player.instance.pet, null);
        if (isSpecialSuccess)
        {
            plant = new Plant(int.Parse(specialEffect.abilityOptionDict.Get("plant", plant.id.ToString())), landId);
        }
        var harvestNum = (int)(Identifier.GetNumIdentifier(plant.ProductNumRange) * prdouctMult);
        var harvest = new Item(plant.id, harvestNum);

        Plant.SaveData(Plant.DefaultPlant(landId), false);
        Item.Add(harvest);
        Item.OpenHintbox(harvest);

        if (isSpecialSuccess)
            Hintbox.OpenHintboxWithContent($"<color=#ffbb33>{name}</color>变异了！", 20);

        return plant;
    }


    public Plant(){}

    public Plant(int id, int landId)
    {
        this.id = id;
        this.landId = landId;
        this.ripeDate = (id == 0) ? DateTime.MaxValue : (DateTime.Now + RipeTime);
        this.productMult = 1;
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

    public Sprite GetIcon(out Vector2 size, out Vector2 posOffset)
    {
        size = Vector2.one * 50;
        posOffset = new Vector2((50 - size.x) / 2, 0);

        if (IsNullOrEmpty(this))
            return null;

        var overrideIcon = SeedInfo.effects.Find(x => x.abilityOptionDict.ContainsKey("icon"));
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

            var sprite = ResourceManager.instance.GetLocalAddressables<Sprite>($"Maps/plant/{iconId}", ItemInfo.IsMod(iconId));
            var offset = overrideIcon.abilityOptionDict.Get("offset")?.ToVector2(delimeter: '/') ?? Vector2.zero;
            size = sprite?.texture.GetTextureSize() ?? size;
            posOffset = new Vector2((50 - size.x) / 2, 0) + offset;
            return sprite;
        }

        return IsRiped ? PlantInfo.icon : SeedInfo.icon;
    }
}


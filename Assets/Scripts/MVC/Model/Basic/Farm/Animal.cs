using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Animal
{
    public static Animal currentAnimal;

    public int id, landId;
    public DateTime ripeDate, lastProduceDate;
    public int productAll, productBorned;

    public string Name => IsRiped ? AnimalInfo?.name : ChildInfo.name;
    public int ChildId => int.Parse(AnimalInfo.options.Get("child", "0"));
    public int ProductId => int.Parse(AnimalInfo.options.Get("product", "0"));
    public int FeedId => int.Parse(ChildInfo.options.Get("feed", "0"));

    public ItemInfo AnimalInfo => Item.GetItemInfo(id);
    public ItemInfo ChildInfo => Item.GetItemInfo(ChildId);
    public ItemInfo ProductInfo => Item.GetItemInfo(ProductId);
    public ItemInfo FeedInfo => Item.GetItemInfo(FeedId);

    public TimeSpan RipeTime => ChildInfo.options.Get("time", "00:00:00").ToTimeSpan();
    public TimeSpan LeftTime => IsRiped ? TimeSpan.Zero : (ripeDate - DateTime.Now);
    public DateTime LastProduceDate => ((id != 0) && (lastProduceDate < BirthDate)) ? BirthDate : lastProduceDate;
    public DateTime BirthDate => ripeDate - RipeTime;

    public string ProductNumRange => AnimalInfo.options.Get("num", "1");
    public string ProduceEachRange => AnimalInfo.options.Get("productEach", "1");
    public int ProductLeft => productAll - productBorned;

    public LandType AnimalLandType => ChildInfo.options.Get("landType", "land").ToLandType();
    public float WalkSpeed => float.Parse(ChildInfo.options.Get("walk", "5"));
    public string Adjective => AnimalInfo.options.Get("adj", "普通的");
    public bool IsRiped => DateTime.Now >= ripeDate;
    public bool IsFollowing => (Player.instance.follower != null) && (Player.instance.follower.landId == landId);
    public bool PauseWhenIdle => ProductId != 63_1001;

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

    public static void OnClick(int landId, Action callback = null)
    {
        var animal = Animal.LoadData(landId);
        if (Animal.IsNullOrEmpty(animal))
            return;

        var itemId = (int)Player.GetSceneData("seed", 0); 
        var itemInfo = Item.GetItemInfo(itemId);
        if ((itemInfo == null) || (itemInfo.type != ItemType.AnimalAction))
        {
            animal.GetSound((sound) => AudioSystem.instance.PlaySound(sound));
            MapManager.instance.SetAnimalPanelActive(true);
            return;
        }

        switch (itemId)
        {
            default:
                Player.SetSceneData("seed", 0);
                break;
            // Feed.
            case 68_0001:
                if (animal.Feed())
                    animal.Produce();
                break;
            // Harverst.
            case 68_0002:
                animal.Harvest();
                break;
            // Follow.
            case 68_0003:
                animal.Follow();
                break;
            // Release. The callback is called after confirm, so we directly return.
            case 68_0099:
                animal.Release(callback);
                return;
        }

        callback?.Invoke();
    }

    public Animal(){}

    public Animal(int id, int landId)
    {
        this.id = id;
        this.landId = landId;
        this.ripeDate = (id == 0) ? DateTime.MaxValue : (DateTime.Now + RipeTime);
        this.lastProduceDate = DateTime.Today;
        this.productAll = (int)Identifier.GetNumIdentifier(ProductNumRange);
        this.productBorned = 0;
    }

    public float GetIdentifier(string id)
    {
        return id switch
        {
            "id" => this.id,
            "landId" => landId,
            "ripe" => IsRiped ? 1 : 0,
            "growth" => (float)(1 - LeftTime / RipeTime),
            _ => Identifier.GetNumIdentifier(id),
        };
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
            var icon = ResourceManager.instance.GetLocalAddressables<Sprite>($"Maps/animal/{iconId}/icon", ItemInfo.IsMod(iconId));
            if (icon != null)
                return icon;
        }

        return IsRiped ? AnimalInfo.icon : ChildInfo.icon;
    }

    public void GetSound(Action<AudioClip> onSuccess, Action<string> onFail = null)
    {
        ResourceManager.instance.GetLocalAddressables<AudioClip>($"BGM/animal/{id}", ItemInfo.IsMod(id), onSuccess, onFail);
    }

    private string GetGifUrl(string dirName, out float speed)
    {
        speed = 1f;

        var overrideGif = ChildInfo?.effects.Find(x => x.abilityOptionDict.ContainsKey("gif"));
        if (overrideGif == null)
            return null;
        
        var gifList = overrideGif.abilityOptionDict.Get("gif").ToIntList('/');
        var gifSpeed = overrideGif.abilityOptionDict.Get("gif_speed", "1").ToFloatList('/');
        var growth = GetGrowth(gifList.Count);

        speed = gifSpeed.Get(growth, gifSpeed.First());

        return $"Maps/animal/{gifList[growth]}/{dirName}.gif";
    }

    public AnimInfo GetGifInfo(string dirName)
    {
        var gifUrl = GetGifUrl(dirName, out var speed);
        if (string.IsNullOrEmpty(gifUrl))
            return null;
        
        var animInfo = new AnimInfo()
        {
            id = gifUrl,
            scale = "1,1",
            size = "anim",
            speed = $"{speed}",
        };

        if (SaveSystem.IsFileExists(animInfo.GifPath))
            return animInfo;

        dirName = dirName.Contains("right") ? dirName.Replace("right", "left") : dirName.Replace("left", "right");
        gifUrl = GetGifUrl(dirName, out speed);
        animInfo = new AnimInfo()
        {
            id = gifUrl,
            scale = "-1,1",
            size = "anim",
            speed = $"{speed}",
        };
        return animInfo;
    }

    public bool IsFollowable(out string reason)
    {
        reason = "该动物不可跟随哦！";

        if ((AnimalLandType == LandType.Water) || (AnimalLandType == LandType.Nest))
            return false;
        
        if ((AnimalLandType == LandType.Insect) && (!IsRiped))
        {
            reason = "未成年的昆虫很脆弱，\n需要细心呵护无法跟随哦！";
            return false;
        }

        return bool.Parse(ChildInfo.options.Get("follow", "true"));
    }

    public void Release(Action callback = null)
    {
        var hintbox = Hintbox.OpenHintboxWithContent($"确定要放生<color=#ffbb33>{Name}</color>吗？放生后无法恢复！", 14);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() =>
        {
            Animal.SaveData(Animal.DefaultAnimal(landId));
            Hintbox.OpenHintboxWithContent($"已放生{Name}", 14); 
            callback?.Invoke();
        });
    }

    public bool Feed()
    {
        var feeder = Item.Find(FeedId);
        if (Item.IsNullOrEmpty(feeder))
        {
            Hintbox.OpenHintboxWithContent($"你没有<color=#ffbb33>{FeedInfo.name}</color>！\n先去购买饲料吧！", 14);
            return false;   
        }

        if (LastProduceDate >= DateTime.Today)
        {
            Hintbox.OpenHintboxWithContent($"这只动物今天已经吃饱了！\n请明天再来！", 14);
            return false;
        }

        Item.Remove(FeedId, 1);
        return true;
    }

    public Item Produce()
    {
        if (!IsRiped)
        {
            ripeDate -= Operator.Min(RipeTime / 8, TimeSpan.FromHours(2));
            lastProduceDate = DateTime.Now;
            Animal.SaveData(this);
            return null;   
        }

        if (ProductLeft <= 0)
        {
            Hintbox.OpenHintboxWithContent($"这只动物已经无法再生产了！\n直接捕获这只动物吧！", 14);
            return null;   
        }

        var num = Mathf.Clamp((int)Identifier.GetNumIdentifier(ProduceEachRange), 1, ProductLeft);
        var item = new Item(ProductId, num);

        if (ProductInfo.id == 63_1001)
        {
            Hintbox.OpenHintboxWithContent($"<color=#ffbb33>{Name}</color>吃饱了！现在可以带她去授粉了！", 14);
        }
        else
        {
            Item.Add(item);
            Item.OpenHintbox(item);
            productBorned += num;
        }

        lastProduceDate = DateTime.Now;
        Animal.SaveData(this);
        return item;
    }

    public Item Harvest()
    {
        if (!IsRiped)
        {
            Hintbox.OpenHintboxWithContent($"这只动物还没有成年，不可以收获哦！", 14);
            return null;
        }

        var specialEffect = AnimalInfo.effects.Find(x => x.abilityOptionDict.ContainsKey("animal"));
        var harvest = new Item(id, 1);

        Animal.SaveData(Animal.DefaultAnimal(landId), false);
        Item.Add(harvest);
        ItemHintbox itemHintbox = Hintbox.OpenHintbox<ItemHintbox>();
        itemHintbox.SetTitle("提示");
        itemHintbox.SetContent($"{harvest.num}只{Adjective}{harvest.name}已经放入仓库", 14, FontOption.Arial);
        itemHintbox.SetOptionNum(1);
        itemHintbox.SetIcon(harvest.icon);
        return harvest;
    }

    public void Follow()
    {
        if (!IsFollowable(out var reason))
        {
            Hintbox.OpenHintboxWithContent(reason, 16);
            return;
        }
        Player.instance.follower = this;
        PlayerController.instance.SetFollowerSprite();
    }

    public enum LandType
    {
        Land = 0,
        Water = 1,
        Insect = 2,
        Nest = 3,
    }
}

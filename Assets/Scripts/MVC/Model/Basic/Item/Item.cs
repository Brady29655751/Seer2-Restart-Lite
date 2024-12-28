using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[XmlRoot("item")]
public class Item
{
    public const int COIN_ID = 1, DIAMOND_ID = 2;

    public static List<Item> petItemDatabase => ItemInfo.database.Where(x => (!skillBookItemDatabase.Select(x => x.id).Contains(x.id)) && 
        (x.effects?.All(y => y.ability == EffectAbility.SetPet) ?? false) && (x.id == x.getId)).Select(x => new Item(x.id, 9999)).ToList();
    public static List<Item> pvpItemDatabase => petItemDatabase.Where(x => (!skillBookItemDatabase.Select(x => x.id).Contains(x.id)) && ((x.effects?.All(y => {
        var banned = new List<string>(){ "exp", "level", "skinId", "iv", "emblem" };
        var type = y.abilityOptionDict.Get("type", "none");
        return (!banned.Contains(type));
    })) ?? false)).ToList();
    public static List<Item> plantItemDatabase => ItemInfo.database.Where(x => x.type == ItemType.Plant)
        .Select(x => new Item(x.id, -1)).ToList();
    public static List<Item> normalPlantItemDatabase => plantItemDatabase.Where(x => !bool.Parse(x.info.options.Get("rare", "false"))).ToList();
    public static List<Item> skillBookItemDatabase => ItemInfo.database.Where(x => (x.id - 10_0000).IsWithin(GameManager.versionData.skillData.minSkillId,
        GameManager.versionData.skillData.maxSkillId) && (x.currencyType == 6)).Select(x => new Item(x.id)).ToList();
    public static List<Item> yiteGrowItemDatabse => ItemInfo.database.Where(x => x.id.IsInRange(9001, 9020)).Select(x => new Item(x.id)).ToList();

    public static List<Item> itemStorage => Player.instance.gameData.itemStorage;
    public ItemInfo info => GetItemInfo(id);
    public string name => info.name;
    public Sprite icon => info.icon;

    [XmlAttribute] public int id { get; set; }
    [XmlAttribute] public int num { get; set; }

    [XmlIgnore] public List<Effect> effects => GetEffects();

    public Item() {

    }

    public Item(int itemId, int amount = 1) {
        id = itemId;
        num = amount;
    }

    public Item(Item rhs) {
        id = rhs.id;
        num = rhs.num;
    }

    public List<Effect> GetEffects() {
        var itemEffects = info.effects.Select(x => new Effect(x)).ToList();
        foreach (var e in itemEffects) {
            e.source = this;
        }
        return itemEffects;
    }

    public static ItemInfo GetItemInfo(int id) {
        return Database.instance.GetItemInfo(id);
    }

    public static ItemHintbox OpenHintbox(Item item) {
        ItemHintbox itemHintbox = Hintbox.OpenHintbox<ItemHintbox>();
        itemHintbox.SetTitle("提示");
        itemHintbox.SetContent(item.num + " 个 " + item.name + " 已经放入背包", 14, FontOption.Arial);
        itemHintbox.SetOptionNum(1);
        itemHintbox.SetIcon(item.icon);
        return itemHintbox;
    }

    public static void AddTo(Item item, List<Item> toWhichStorage) {
        var addItem = new Item(item.info.getId, item.num);
        int index = toWhichStorage.FindIndex(x => x.id == addItem.id);
        if (index != -1) {
            toWhichStorage[index].num += addItem.num;
        } else {
            toWhichStorage.Add(addItem);
        }
        SaveSystem.SaveData();
    }

    public static void RemoveFrom(int id, int num, List<Item> toWhichStorage) {
        int index = toWhichStorage.FindIndex(x => x.id == id);
        if (index != -1) {
            Item item = toWhichStorage[index];
            item.num -= num;
            if (item.num <= 0) {
                toWhichStorage.Remove(item);
            }
            SaveSystem.SaveData();
            return;
        } 
        throw new Exception("Item was not in bag originally.");
    }

    public static Item Find(int id, List<Item> toWhichStorage = null) {
        toWhichStorage ??= itemStorage;
        return toWhichStorage.Find(x => x.id == id);
    }

    public static void Add(Item item) {
        var addItem = new Item(item.info.getId, item.num);
        int index = itemStorage.FindIndex(x => x.id == addItem.id);
        if (index != -1) {
            itemStorage[index].num += addItem.num;
        } else {
            itemStorage.Add(addItem);
        }
        SaveSystem.SaveData();
    }

    public static void Remove(int id, int num = 1) {
        int index = itemStorage.FindIndex(x => x.id == id);
        if (index != -1) {
            Item item = itemStorage[index];
            item.num -= num;
            if (item.num <= 0) {
                itemStorage.Remove(item);
            }
            SaveSystem.SaveData();
            return;
        } 
        throw new Exception("Item was not in bag originally.");
    }

    public static void Buy(int id, int num, Action onAfterBuy = null, List<Item> toWhichStorage = null) {
        ItemInfo info = GetItemInfo(id);
        ItemInfo currencyInfo = GetItemInfo(info.currencyType);
        int total = info.price * num;

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        Item currency = Item.Find(info.currencyType, toWhichStorage);
        if ((currency == null) || (currency.num < total)) {
            hintbox.SetContent(currencyInfo.name + "不足无法购买", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        Item price = new Item(currency.id, total);
        Item item = new Item(id, num);
        hintbox.SetContent("确定要购买 " + num + " 个 " + info.name + " 吗", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => OnBuySuccess(price, item, onAfterBuy, toWhichStorage));
    }

    private static void OnBuySuccess(Item price, Item item, Action callback, List<Item> toWhichStorage = null) {
        if (toWhichStorage == null) {
            Item.Remove(price.id, price.num);
            Item.Add(item);
        } else {
            Item.RemoveFrom(price.id, price.num, toWhichStorage);
            Item.AddTo(item, toWhichStorage);
        }

        callback?.Invoke();

        Hintbox successHintbox = Hintbox.OpenHintbox();
        successHintbox.SetTitle("提示");
        successHintbox.SetContent("购买成功", 14, FontOption.Arial);
        successHintbox.SetOptionNum(1);
    }

    public static void Sell(int id, int num, Action onAfterSell = null, List<Item> toWhichStorage = null) {
        ItemInfo info = GetItemInfo(id);
        int total = info.price * num;

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        Item item = Item.Find(id, toWhichStorage);
        if ((item == null) || (item.num < num)) {
            hintbox.SetContent("物品不足无法出售", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        Item price = new Item(info.currencyType, total);
        hintbox.SetContent("确定要出售 " + num + " 个 " + item.name + " 吗", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => OnSellSuccess(price, new Item(item.id, num), onAfterSell, toWhichStorage));
    }

    private static void OnSellSuccess(Item price, Item item, Action callback, List<Item> toWhichStorage = null) {
        if (toWhichStorage == null) {
            Item.Remove(item.id, item.num);
            Item.Add(price);
        } else {
            Item.RemoveFrom(item.id, item.num, toWhichStorage);
            Item.AddTo(price, toWhichStorage);
        }

        callback?.Invoke();

        Hintbox successHintbox = Hintbox.OpenHintbox();
        successHintbox.SetTitle("提示");
        successHintbox.SetContent("出售成功", 14, FontOption.Arial);
        successHintbox.SetOptionNum(1);
    }

    public bool IsInCategory(ItemCategory category) {
        return info.type.IsInCategory(category);
    }

    public bool IsUsable(object invokeUnit, BattleState state) {
        if (num <= 0)
            return false;
        
        EffectHandler handler = new EffectHandler();
        handler.AddEffects(invokeUnit, effects);
        return handler.Condition(state, state == null).Any(x => x) && (GetMaxUseCount(invokeUnit, state) > 0);
    }

    public int Use(object invokeUnit, BattleState state, int useCount = 1, PetBagMode petBagMode = PetBagMode.Normal) {
        int count = useCount;

        if ((info.type == ItemType.EXP) && (!effects.Exists(x => x.abilityOptionDict.ContainsKey("max_use"))))
            count = this.UseExp(invokeUnit, state, useCount);
        else
            count = this.UseDefault(invokeUnit, state, useCount, petBagMode);

        if ((petBagMode != PetBagMode.Normal) && (petBagMode != PetBagMode.YiTeRogue))
            return count;

        if (info.removable) {
            if (petBagMode == PetBagMode.Normal)
                Item.Remove(id, count);
            else
                Item.RemoveFrom(id, count, YiTeRogueData.instance.itemBag);
        } else
            SaveSystem.SaveData();
        
        return count;
    }

    public int GetMaxUseCount(object invokeUnit, BattleState state) {
        if (effects.Exists(x => x.abilityOptionDict.ContainsKey("max_use")))
            return this.StuffMaxUseCount(invokeUnit, state);

        return info.type switch {
            ItemType.HpPotion => this.HpPotionMaxUseCount(invokeUnit, state),
            ItemType.Evolve => 1,
            ItemType.Emblem => 1,
            ItemType.EXP => this.ExpMaxUseCount(invokeUnit, state),
            ItemType.EV => this.EvMaxUseCount(invokeUnit, state),
            ItemType.IV => 1,
            ItemType.Personality => 1,
            ItemType.Skill => 1,
            ItemType.Buff => 1,
            ItemType.Capture => 1,
            ItemType.Stuff => this.StuffMaxUseCount(invokeUnit, state),
            _ => int.MaxValue
        };
    }

}
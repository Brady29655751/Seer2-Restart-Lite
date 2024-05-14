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

    private List<Effect> GetEffects() {
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

    public static Item Find(int id) {
        return itemStorage.Find(x => x.id == id);
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

    public static void Buy(int id, int num, Action onAfterBuy = null) {
        ItemInfo info = GetItemInfo(id);
        int total = info.price * num;

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        Item currency = Item.Find(info.currencyType);
        if ((currency == null) || (currency.num < total)) {
            hintbox.SetContent(currency.name + "不足无法购买", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        Item price = new Item(currency.id, total);
        Item item = new Item(id, num);
        hintbox.SetContent("确定要购买 " + num + " 个 " + info.name + " 吗", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => OnBuySuccess(price, item, onAfterBuy));
    }

    private static void OnBuySuccess(Item price, Item item, Action callback) {
        Item.Remove(price.id, price.num);
        Item.Add(item);

        callback?.Invoke();

        Hintbox successHintbox = Hintbox.OpenHintbox();
        successHintbox.SetTitle("提示");
        successHintbox.SetContent("购买成功", 14, FontOption.Arial);
        successHintbox.SetOptionNum(1);
    }

    public static void Sell(int id, int num, Action onAfterSell = null) {
        ItemInfo info = GetItemInfo(id);
        int total = info.price * num;

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        Item currency = Item.Find(info.currencyType);
        Item item = Item.Find(id);
        if ((item == null) || (item.num < num)) {
            hintbox.SetContent("物品不足无法出售", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        Item price = new Item(currency.id, total);
        hintbox.SetContent("确定要出售 " + num + " 个 " + item.name + " 吗", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => OnSellSuccess(price, new Item(item.id, num), onAfterSell));
    }

    private static void OnSellSuccess(Item price, Item item, Action callback) {
        Item.Remove(item.id, item.num);
        Item.Add(price);

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
        return (handler.Condition(state, false).Any(x => x)) && (GetMaxUseCount(invokeUnit, state) > 0);
    }

    public int Use(object invokeUnit, BattleState state, int useCount = 1) {
        int count = info.type switch {
            ItemType.EXP => this.UseExp(invokeUnit, state, useCount),
            _ => this.UseDefault(invokeUnit, state, useCount)
        };
        if (info.removable) {
            Item.Remove(id, count);
        }
        return count;
    }

    public int GetMaxUseCount(object invokeUnit, BattleState state) {
        return info.type switch {
            ItemType.HpPotion => this.HpPotionMaxUseCount(invokeUnit, state),
            ItemType.EXP => this.ExpMaxUseCount(invokeUnit, state),
            ItemType.EV => this.EvMaxUseCount(invokeUnit, state),
            ItemType.IV => 1,
            ItemType.Personality => 1,
            ItemType.Skill => 1,
            ItemType.Capture => 1,
            ItemType.Stuff => this.StuffMaxUseCount(invokeUnit, state),
            _ => int.MaxValue
        };
    }

}
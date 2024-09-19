using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShopPanel : Panel
{
    [SerializeField] protected ItemShopType shopType;
    [SerializeField] protected ItemShopController shopController;
    [SerializeField] protected IButton buyButton, sellButton;

    protected ItemShopMode shopMode = ItemShopMode.Buy;

    protected Dictionary<ItemShopType, string> shopNameDict = new Dictionary<ItemShopType, string>() {
        { ItemShopType.None, "道具商店" },
        { ItemShopType.PetPotion, "精灵道具商店" },
        { ItemShopType.Mine, "矿石回收商店" },
        { ItemShopType.Honor, "荣誉商店" },
        { ItemShopType.Sign, "签到商店" },
    };

    protected Dictionary<ItemShopType, List<int>> shopItemIdDict = new Dictionary<ItemShopType, List<int>>() {
        { ItemShopType.None, new List<int>() },
        { ItemShopType.PetPotion, new List<int>() { 
            21010, 20001, 20002, 
            10238, 10239, 10101, 10111, 10211,
            110050,
            10011, 10012, 10013, 10014, 10015, 10016, 
            10001, 10002, 10003, 10004, 10005, 
        } },
        { ItemShopType.Mine, new List<int>() { 
            1001,   1002,   1003,   1004,
        } },
        { ItemShopType.Honor, new List<int>() { 
            8001,   8002,   8003,   8004,   8005,
            300048,
        } },
        { ItemShopType.Sign, new List<int>() {
            500178, 500436, 500959, 500826
        } },
    };

    protected override void Awake()
    {
        base.Awake();
        shopController.onItemSellEvent += (item) => SetStorage();
    }

    public override void Init()
    {
        base.Init();
        SetShopMode(shopMode);
        SetShopType(shopType);
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                break;
            case "title":
                shopNameDict.Set(ItemShopType.Others, param);
                SetTitle();
                break;
            case "mode":
                var mode = param switch {
                    "buy" => ItemShopMode.Buy,
                    "sell" => ItemShopMode.Sell,
                    _ => ItemShopMode.Buy,
                };
                SetShopMode(mode);
                break;
            case "type":
                var type = param switch {
                    "pet_potion" => ItemShopType.PetPotion,
                    "mine" => ItemShopType.Mine,
                    "honor" => ItemShopType.Honor,
                    "sign"  => ItemShopType.Sign,
                    "others" => ItemShopType.Others,
                    _ => ItemShopType.None,
                };
                SetShopType(type);
                break;
            case "currency":
                var currency = param.ToIntList('/');
                SetCurrencyType(currency[0], currency[1]);
                break;
            case "item":
                var itemList = param.ToIntList('/');
                shopItemIdDict.Set(ItemShopType.Others, itemList);
                SetShopType(ItemShopType.Others);
                break;
        }
    }

    public void SetShopMode(ItemShopMode shopMode) {
        this.shopMode = shopMode;
        buyButton?.gameObject.SetActive(shopMode == ItemShopMode.Buy);
        sellButton?.gameObject.SetActive(shopMode == ItemShopMode.Sell);
    }

    public void SetShopType(ItemShopType shopType) {
        this.shopType = shopType;
        SetTitle();
        SetStorage();
    }

    public void SetCurrencyType(int coinType, int diamondType) {
        shopController.SetCurrencyType(coinType, diamondType);
    }

    private void SetTitle() {
        shopController.SetTitle(shopNameDict.Get(shopType, "道具商店"));
    }

    private void SetStorage() {
        var idList = shopItemIdDict.Get(shopType, new List<int>());
        var itemList = idList.Select(x => new Item(x, GetStorageItemCount(x))).ToList();
        shopController.SetStorage(itemList);
    }

    private int GetStorageItemCount(int id) {
        return shopMode switch {
            ItemShopMode.Sell => Item.Find(id)?.num ?? 0,
            _ => -1
        };
    }

}

public enum ItemShopMode {
    Buy,
    Sell,
}

public enum ItemShopType {
    Others = -1,
    None = 0,
    PetPotion = 1,
    Mine = 2,
    Honor = 3,
    Sign = 4,
}
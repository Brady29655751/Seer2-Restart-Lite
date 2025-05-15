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
    protected List<int> itemNumLimit = new List<int>();

    protected Dictionary<ItemShopType, string> shopNameDict = new Dictionary<ItemShopType, string>() {
        { ItemShopType.None, "道具商店" },
        { ItemShopType.PetPotion, "精灵道具商店" },
        { ItemShopType.Mine, "矿石回收商店" },
        { ItemShopType.Honor, "荣誉商店" },
        { ItemShopType.Sign, "签到商店" },
        { ItemShopType.YiTe, "伊特商店" },
        { ItemShopType.Plant, "农作物出售" },
        { ItemShopType.Achievement, "称号商店" },
    };

    protected Dictionary<ItemShopType, List<int>> shopItemIdDict = new Dictionary<ItemShopType, List<int>>() {
        { ItemShopType.None, new List<int>() },
        { ItemShopType.PetPotion, new List<int>() { 
            10101, 10111, 10221, 10238, 10239, 20001, 10018, 20002, 10211, 
            10242, 21006, 21007, 21008, 21009, 21010, 10011, 10012, 10013, 
            10014, 10015, 10016, 10001, 10002, 10003, 10004, 10005, 110050, 
        } },
        { ItemShopType.Mine, ItemInfo.database.Where(x => x.type == ItemType.Mine).Select(x => x.id).ToList() },
        { ItemShopType.Honor, new List<int>() { 
            8001,   8002,   8003,   8004,   8005,
            8006,   8007,   8008,
            300048, 300058, 300077, 300174, 300176,
        } },
        { ItemShopType.Sign, new List<int>() {
            500000, 500178, 500436, 500959, 500826, 
            500761, 500280, 500307, 500809, 500179,
            500538, 501261, 501262,
        } },
        { ItemShopType.YiTe, new List<int>() {
            500091, 24000,  320092, 320093, 320094,
            320095, 320096, 320100, 320810,
        } },
        { ItemShopType.Plant, Item.plantItemDatabase.Select(x => x.id).ToList() },
        { ItemShopType.Achievement, Item.achievementItemDatabase.Select(x => x.id).ToList() },
    };

    public static bool IsBuy(ItemShopMode shopMode) {
        return (shopMode == ItemShopMode.Buy) || (shopMode == ItemShopMode.BuyYiTe);
    }

    public static bool IsSell(ItemShopMode shopMode) {
        return (shopMode == ItemShopMode.Sell) || (shopMode == ItemShopMode.SellYiTe);
    }

    public static bool IsYite(ItemShopMode shopMode) {
        return (shopMode == ItemShopMode.BuyYiTe) || (shopMode == ItemShopMode.SellYiTe);
    }

    protected override void Awake()
    {
        base.Awake();
        shopController.onItemBuyEvent += (item) => SetStorage();
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
                    "buy_yite" => ItemShopMode.BuyYiTe,
                    "sell_yite" => ItemShopMode.SellYiTe,
                    _ => ItemShopMode.Buy,
                };
                SetShopMode(mode);
                break;
            case "type":
                var type = param switch {
                    "pet_potion" => ItemShopType.PetPotion,
                    "mine"  => ItemShopType.Mine,
                    "honor" => ItemShopType.Honor,
                    "sign"  => ItemShopType.Sign,
                    "yite"  => ItemShopType.YiTe,
                    "plant" => ItemShopType.Plant,
                    "achievement" => ItemShopType.Achievement,
                    "others"=> ItemShopType.Others,
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
            case "limit":
                itemNumLimit = param.ToIntList('/');
                SetStorage();
                break;
        }
    }

    public void SetShopMode(ItemShopMode shopMode) {
        this.shopMode = shopMode;
        buyButton?.gameObject.SetActive(IsBuy(shopMode));
        sellButton?.gameObject.SetActive(IsSell(shopMode));
        shopController.SetShopMode(shopMode);
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
        var itemList = idList.Select((x, i) => new Item(x, GetStorageItemCount(x, i))).ToList();
        shopController.SetStorage(itemList);
    }

    private int GetStorageItemCount(int id, int index) {
        if (!ListHelper.IsNullOrEmpty(itemNumLimit))
            return (itemNumLimit[index] < 0) ? -1 : Mathf.Max(0, itemNumLimit[index] - int.Parse(Activity.Shop.GetData(id.ToString(), "0")));

        var storage = IsYite(shopMode) ? YiTeRogueData.instance.itemBag : null;
        return IsSell(shopMode) ? (Item.Find(id, storage)?.num ?? 0) : -1;
    }

}

public enum ItemShopMode {
    Buy,
    Sell,
    BuyYiTe,
    SellYiTe,
}

public enum ItemShopType {
    Others = -1,
    None = 0,
    PetPotion = 1,
    Mine = 2,
    Honor = 3,
    Sign = 4,
    YiTe = 5,
    Plant = 6,
    Achievement = 7,
}
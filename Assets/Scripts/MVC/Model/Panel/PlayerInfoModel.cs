using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoModel : Module
{
    private GameData data => Player.instance.gameData;
    private List<Item> itemStorage => ItemShopPanel.IsYite(shopMode) ? YiTeRogueData.instance.itemBag : null;
    protected ItemShopMode shopMode = ItemShopMode.Buy;
    public int coinType { get; private set; } = 1;
    public int diamondType { get; private set; } = 2;
    public int coin => (coinType == 0) ? -1 : (Item.Find(coinType, itemStorage)?.num ?? 0);
    public int diamond => (diamondType == 0) ? -1 : (Item.Find(diamondType, itemStorage)?.num ?? 0);

    public void SetShopMode(ItemShopMode shopMode) {
        this.shopMode = shopMode;
    }

    public void SetCurrencyType(int coinType = 1, int diamondType = 2) {
        this.coinType = coinType;
        this.diamondType = diamondType; 
    }

    public void OnChangeNameSuccess(string newName) {
        data.nickname = newName;
        SaveSystem.SaveData();
    }

}

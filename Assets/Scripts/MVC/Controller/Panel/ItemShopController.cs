using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShopController : Module
{
    [SerializeField] private PlayerInfoController playerInfoController;
    [SerializeField] private PetItemController itemController;
    [SerializeField] private SelectNumController selectNumController;
    [SerializeField] private ItemDetailView itemDetailView;

    public event Action<Item> onItemSellEvent;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    private void InitSelectSubscriptions() {
        itemController.onItemSelectEvent += itemDetailView.SetItem;
        itemController.onItemSelectEvent += (x) => OnItemNumChange(selectNumController.GetInputValue());
        selectNumController.onValueChangedEvent += OnItemNumChange;
    }   

    public void SetCurrencyType(int coinType, int diamondType) {
        playerInfoController.SetCurrencyType(coinType, diamondType);
    }

    public void SetTitle(string title) {
        itemDetailView.SetTitle(title);
    }

    public void SetStorage(List<Item> itemBag) {
        itemController.SetItemBag(itemBag);
        itemController.Select(0);
        selectNumController.SetMaxValue(99);
        playerInfoController.ShowCurrency();
    }

    public void OnItemNumChange(int num = -1) {
        if (num == -1)
            return;

        Item item = itemController.GetSelectedItem().FirstOrDefault();
        if (item == null)
            return;

        itemDetailView.SetTotal((uint)(num * item.info.price), item.info.currencyInfo?.name);
    }

    public void OnBuyItem() {
        Item item = itemController.GetSelectedItem().FirstOrDefault();
        if (item == null)
            return;

        int num = selectNumController.GetInputValue();
        Action onAfterBuy = playerInfoController.ShowCurrency;

        Item.Buy(item.id, num, onAfterBuy);
    }

    public void OnSellItem() {
        Item item = itemController.GetSelectedItem().FirstOrDefault();
        if (item == null)
            return;

        int num = selectNumController.GetInputValue();
        Action onAfterSell = () => {
            playerInfoController.ShowCurrency();
            onItemSellEvent?.Invoke(new Item(item.id, num));
        };

        Item.Sell(item.id, num, onAfterSell);
    }
    
}

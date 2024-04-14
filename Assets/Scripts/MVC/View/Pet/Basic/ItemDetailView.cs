using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailView : Module
{
    [SerializeField] private IText titleText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text totalText;
    [SerializeField] private Text descriptionText;

    public void SetItem(Item item) {
        nameText?.SetText(item.name);
        priceText?.SetText(item.info.price + " " + item.info.currencyInfo?.name);
        descriptionText?.SetText(item.info.effectDescription);
    }

    public void SetTitle(string title) {
        titleText?.SetText(title);
    }

    public void SetTotal(uint total, string currencyType) {
        totalText?.SetText(total + " " + currencyType);
    }

}

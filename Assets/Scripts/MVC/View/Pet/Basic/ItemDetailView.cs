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
    [SerializeField] private Text otherInfoText;

    public void SetItem(Item item) {
        var priceDesc = item.info.price + " " + item.info.CurrencyInfo?.name;
        var effectDesc = (item.info.removable ? string.Empty : "<color=#ffbb33>【无限再生】</color>\n") + item.info.GetEffectDescription(false);
        nameText?.SetText(item.name);
        priceText?.SetText(priceDesc);
        descriptionText?.SetText(effectDesc);
    }

    public void SetTitle(string title) {
        titleText?.SetText(title);
    }

    public void SetTotal(uint total, string currencyType) {
        totalText?.SetText(total + " " + currencyType);
    }

    public void SetDescription(string description)
    {
        descriptionText?.SetText(description);
    }

    public void SetDescriptionFontSize(int fontSize)
    {
        if (descriptionText == null)
            return;

        descriptionText.fontSize = fontSize;
    }

    public void SetOtherInfo(string info) {
        otherInfoText?.SetText(info);
    }

}

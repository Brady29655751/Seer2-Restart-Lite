using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCell : UIModule
{
    public RectTransform rectTransform;
    [SerializeField] private IButton button;
    [SerializeField] private TextCell idCell, nameCell, typeCell, priceCell, currencyCell, habitatCell;
    [SerializeField] private Image iconImage;

    public ItemInfo currentItemInfo { get; private set; }

    public void SetItemInfo(ItemInfo itemInfo) {
        currentItemInfo = itemInfo;
        gameObject.SetActive(itemInfo != null);
        if (itemInfo == null)
            return;

        idCell.SetText(itemInfo.id.ToString());
        nameCell.SetText(itemInfo.name);
        typeCell.SetText(itemInfo.type.ToTypeName());
        typeCell.SetTextColor(ColorHelper.gold);
        priceCell.SetText(itemInfo.price.ToString());
        currencyCell.SetText(itemInfo.currencyType.ToString());
        habitatCell.SetText(itemInfo.habitat);
        habitatCell.SetTextColor(ColorHelper.green);
        habitatCell.SetCallback(GetItem);
        iconImage.SetSprite(itemInfo.icon);
    }

    public void SetInfoPrompt(InfoPrompt prompt) {
        infoPrompt = prompt;
    }

    public void SetCallback(Action<ItemInfo> callback, string which = null) {
        Action itemCallback = () => {
            callback?.Invoke(currentItemInfo);
            SetInfoPromptActive(false);
        };

        switch (which) {
            default:
                button?.onPointerClickEvent?.SetListener(itemCallback.Invoke);
                return;
            case "id":
                idCell?.SetCallback(itemCallback);
                return;
        }
    }

    public void ShowItemInfo() {
        infoPrompt?.SetItem(new Item(currentItemInfo.id, -1));
    }

    private void GetItem() {
        if (currentItemInfo == null)
            return;

        if (currentItemInfo.linkId == "Workshop") {
            var item = new Item(currentItemInfo.id);
            Item.Add(item);
            Item.OpenHintbox(item);
            return;
        }
            
        Panel.Link(currentItemInfo.linkId);
    }

}

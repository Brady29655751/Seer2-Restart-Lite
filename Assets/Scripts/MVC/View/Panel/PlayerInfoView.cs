using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoView : Module
{
    [SerializeField] private Image coinImage, diamondImage;
    [SerializeField] private Text coinText, diamondText;

    public void SetCoin(int coin) {
        if (coinText == null)
            return;

        coinText.text = (coin < 0) ? string.Empty : coin.ToString();
    }

    public void SetDiamond(int diamond) {
        if (diamondText == null)
            return;

        diamondText.text = (diamond < 0) ? string.Empty : diamond.ToString();
    }

    public void SetCurrency(int coin, int diamond) {
        SetCoin(coin);
        SetDiamond(diamond);
    }

    public void SetCurrencyType(int coinType, int diamondType) {
        coinImage?.gameObject.SetActive(coinType > 0);
        diamondImage?.gameObject.SetActive(diamondType > 0);

        coinImage?.SetSprite(Item.GetItemInfo(coinType)?.icon);
        diamondImage?.SetSprite(Item.GetItemInfo(diamondType)?.icon);
    }

    public void OnChangeNameEmpty() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("昵称不能空白", 20, FontOption.Zongyi);
    }
}

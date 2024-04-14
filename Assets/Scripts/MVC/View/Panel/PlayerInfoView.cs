using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoView : Module
{
    [SerializeField] private Text coinText;
    [SerializeField] private Text diamondText;

    public void SetCoin(int coin) {
        if (coinText == null)
            return;

        coinText.text = coin.ToString();
    }

    public void SetDiamond(int diamond) {
        if (diamondText == null)
            return;

        diamondText.text = diamond.ToString();
    }

    public void SetCurrency(int coin, int diamond) {
        SetCoin(coin);
        SetDiamond(diamond);
    }

    public void OnChangeNameEmpty() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("昵称不能空白", 20, FontOption.Zongyi);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoController : Module
{
    [SerializeField] private PlayerInfoModel infoModel;
    [SerializeField] private PlayerInfoView infoView;

    public override void Init() {
        SetCurrencyType(infoModel.coinType, infoModel.diamondType);
    }

    public void SetShopMode(ItemShopMode shopMode) {
        infoModel.SetShopMode(shopMode);
    }

    public void SetCurrencyType(int coinType, int diamondType) {
        infoModel.SetCurrencyType(coinType, diamondType);
        infoView.SetCurrencyType(infoModel.coinType, infoModel.diamondType);
        ShowCurrency();
    }

    public void ShowCurrency() {
        infoView.SetCurrency(infoModel.coin, infoModel.diamond);
    }

    public void ShowCurrency(int coin, int diamond) {
        infoView.SetCurrency(infoModel.coin, infoModel.diamond);
    }

    public void OnChangeNameEmpty() {
        infoView.OnChangeNameEmpty();
    }

    public void OnChangeNameSuccess(string newName) {
        infoModel.OnChangeNameSuccess(newName);
    }

}

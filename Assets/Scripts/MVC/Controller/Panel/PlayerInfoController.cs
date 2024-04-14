using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoController : Module
{
    [SerializeField] private PlayerInfoModel infoModel;
    [SerializeField] private PlayerInfoView infoView;

    public void ShowCurrency() {
        infoView.SetCurrency(infoModel.coin, infoModel.diamond);
    }

    public void OnChangeNameEmpty() {
        infoView.OnChangeNameEmpty();
    }

    public void OnChangeNameSuccess(string newName) {
        infoModel.OnChangeNameSuccess(newName);
    }

}

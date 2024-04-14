using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : Panel
{
    private GameData data => Player.instance.gameData;
    [SerializeField] private ChangeNameController changeNameController;
    [SerializeField] private PlayerInfoController infoController;

    protected override void Awake()
    {
        base.Awake();
        InitChangeNameSubscriptions();
    }

    public override void Init() {
        base.Init();
        InitPlayerData();
    }

    private void InitPlayerData() {
        changeNameController.InitName(data.nickname);
        infoController.ShowCurrency();
    }

    private void InitChangeNameSubscriptions() {
        changeNameController.onChangeNameEmptyEvent += infoController.OnChangeNameEmpty;
        changeNameController.onChangeNameSuccessEvent += infoController.OnChangeNameSuccess;

        //! Lazy check
        if (PlayerController.instance != null) {
            changeNameController.onChangeNameSuccessEvent += PlayerController.instance.SetPlayerName;
        }
    }

}

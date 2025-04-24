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
    [SerializeField] private Image playerImage;
    [SerializeField] private Sprite nonoSprite, robotSprite;

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
        changeNameController.InitAchievement(data.achievement);
        infoController.ShowCurrency();
        SetPlayerSprite();
    }

    private void InitChangeNameSubscriptions() {
        changeNameController.onChangeNameEmptyEvent += infoController.OnChangeNameEmpty;
        changeNameController.onChangeNameSuccessEvent += infoController.OnChangeNameSuccess;

        //! Lazy check
        if (PlayerController.instance != null) {
            changeNameController.onChangeNameSuccessEvent += PlayerController.instance.SetPlayerName;
            changeNameController.onChangeAchievementSuccessEvent += PlayerController.instance.SetPlayerAchievement;
        }
    }

    private void SetPlayerSprite() {
        var sprite = data.settingsData.useRobotAsPlayer ? robotSprite : nonoSprite;
        playerImage?.SetSprite(sprite);
        playerImage?.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.GetResizedWidth(100));
    }

    public void ChangePlayerRobot() {
        data.settingsData.useRobotAsPlayer = !data.settingsData.useRobotAsPlayer;
        SetPlayerSprite();
        SaveSystem.SaveData();
    }

    public void ChangeShootMode() {
        Player.instance.isShootMode = !Player.instance.isShootMode;
    }

}

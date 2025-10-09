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
    [SerializeField] private PetItemController itemController;
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
        itemController.SetItemBag(data.itemBag);
        itemController.gameObject.SetActive(false);
        SetPlayerSprite();
    }

    private void InitChangeNameSubscriptions() {
        changeNameController.onChangeNameEmptyEvent += infoController.OnChangeNameEmpty;
        changeNameController.onChangeNameSuccessEvent += infoController.OnChangeNameSuccess;
        changeNameController.onChangeAchievementSuccessEvent += (x) => SetPlayerSprite();

        //! Lazy check
        if (PlayerController.instance != null)
        {
            changeNameController.onChangeNameSuccessEvent += PlayerController.instance.SetPlayerName;
            changeNameController.onChangeAchievementSuccessEvent += PlayerController.instance.SetPlayerAchievement;
        }
    }

    private ItemInfo GetPlayerEquiment()
    {
        var item = Item.GetItemInfo(Player.instance.gameData.achievement);
        if (item == null)
            return null;

        return (item.type == ItemType.Equipment) ? item : null;
    }

    private void SetPlayerSprite()
    {
        var equipment = GetPlayerEquiment();
        var sprite = data.settingsData.useRobotAsPlayer ? robotSprite : (equipment?.icon ?? nonoSprite);
        var height = (data.settingsData.useRobotAsPlayer || (equipment == null)) ? 100 : 150;

        playerImage?.SetSprite(sprite);
        playerImage?.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.GetResizedWidth(height));
        playerImage?.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
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

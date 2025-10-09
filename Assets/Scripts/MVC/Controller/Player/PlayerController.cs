using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : Manager<PlayerController>
{
    [SerializeField] private PlayerModel playerModel;
    [SerializeField] private PlayerView nonoView;
    [SerializeField] private RobotView robotView;
    private PlayerView playerView;

    private void FixedUpdate()
    {
        playerModel.OnPlayerMove();
        playerView.SetPlayerPosition(playerModel.currentPos);
        if (!playerModel.isMoving)
            playerView.SetDirection(new Vector2(0, 0));
    }

    public void OnClickMap(BaseEventData bed)
    {
        var ped = (PointerEventData)bed;
        switch (ped.pointerId)
        {
            default:
                if (Player.instance.isShootMode)
                    Shoot();
                else
                    SetDestinationByMousePos();
                return;
            case -2:
                Player.instance.isShootMode = !Player.instance.isShootMode;
                return;
        }
    }

    public void SetMap(Map map)
    {
        playerModel.SetMap(map);
        nonoView.gameObject.SetActive(!playerModel.useRobot);
        robotView.gameObject.SetActive(playerModel.useRobot);
        playerView = playerModel.useRobot ? robotView : nonoView;

        var item = Item.GetItemInfo(Player.instance.gameData.achievement);
        if ((item != null) && (item.type == ItemType.Equipment))
        {
            nonoView.SetPlayerSprite(item.icon);
            nonoView.SetPlayerSize(item.icon.GetResizedSize(100 * Vector2.one));
        }
    }

    public void Shoot(List<Action> callbacks = null)
    {
        if (!Player.instance.isShootMode)
            return;

        var item = Player.instance.gameData.achievement;
        var pos = playerModel.GetCanvasPosByMousePos(Input.mousePosition);
        playerView.Shoot((item == 0) ? 1 : item, pos, callbacks);
    }

    public void SetDestinationByMousePos()
    {
        playerModel.SetDestinationByMousePos(Input.mousePosition, null);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetDestinationByMousePos(Action onArrive = null)
    {
        playerModel.SetDestinationByMousePos(Input.mousePosition, onArrive);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetDestinationByCanvasPos(Vector2 canvasPos, Action onArrive = null)
    {
        playerModel.SetDestinationByCanvasPos(canvasPos, onArrive);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetPlayerPositionByMapPos(Vector2 mapPos, Vector2 anchor)
    {
        playerModel.SetPlayerPositionByMapPos(mapPos, anchor);
        playerView.SetPlayerPosition(playerModel.currentPos);
    }

    public void SetPlayerPosition(Vector2 canvasPos)
    {
        playerModel.SetPlayerPosition(canvasPos);
        playerView.SetPlayerPosition(playerModel.currentPos);
    }

    public void SetPlayerName(string newName)
    {
        playerView.SetPlayerName(newName);
    }

    public void SetPlayerAchievement(string newAchievement)
    {
        playerView.SetPlayerAchievement(newAchievement);
    }

    public void OpenPlayerInfoPanel()
    {
        playerView.OpenPlayerInfoPanel();
    }

    public void SetPlayerSprite(string spriteName)
    {
        SetPlayerSprite(NpcInfo.GetIcon(spriteName));
    }

    public void SetPlayerSprite(Sprite sprite)
    {
        if (sprite == null)
            return;

        playerView.SetPlayerSprite(sprite);
    }

}

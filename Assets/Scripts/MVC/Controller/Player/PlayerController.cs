using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Manager<PlayerController>
{
    [SerializeField] private PlayerModel playerModel;
    [SerializeField] private PlayerView nonoView;
    [SerializeField] private RobotView robotView;
    private PlayerView playerView;

    private void FixedUpdate() {
        playerModel.OnPlayerMove();
        playerView.SetPlayerPosition(playerModel.currentPos);
        if (!playerModel.isMoving)
            playerView.SetDirection(new Vector2(0, 0));
    }

    public void SetMap(Map map) {
        playerModel.SetMap(map);
        nonoView.gameObject.SetActive(!playerModel.useRobot);
        robotView.gameObject.SetActive(playerModel.useRobot);
        playerView = playerModel.useRobot ? robotView : nonoView;
    }

    public void SetDestinationByMousePos() { 
        playerModel.SetDestinationByMousePos(Input.mousePosition, null);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetDestinationByMousePos(Action onArrive = null) {
        playerModel.SetDestinationByMousePos(Input.mousePosition, onArrive);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetDestinationByCanvasPos(Vector2 canvasPos, Action onArrive = null) {
        playerModel.SetDestinationByCanvasPos(canvasPos, onArrive);
        playerView.SetDirection(playerModel.direction);
    }

    public void SetPlayerPositionByMapPos(Vector2 mapPos, Vector2 anchor) {
        playerModel.SetPlayerPositionByMapPos(mapPos, anchor);
        playerView.SetPlayerPosition(playerModel.currentPos);
    }

    public void SetPlayerPosition(Vector2 canvasPos) {
        playerModel.SetPlayerPosition(canvasPos);
        playerView.SetPlayerPosition(playerModel.currentPos);
    }

    public void SetPlayerName(string newName) {
        playerView.SetPlayerName(newName);
    }

    public void SetPlayerAchievement(string newAchievement) {
        playerView.SetPlayerAchievement(newAchievement);
    }

    public void OpenPlayerInfoPanel() {
        playerView.OpenPlayerInfoPanel();
    }
    
   
    public void SetPlayerSprite(string spriteName)
    {
        // 直接透過 NpcInfo.GetIcon 自動加載對應 Npc 資源
        Sprite sprite = NpcInfo.GetIcon(spriteName);

        if (sprite != null) {
            // 如果成功加载到 Sprite，将其传递给 playerView.SetPlayerSprite 方法
            playerView.SetPlayerSprite(sprite);
        } else{
            Debug.LogError("Failed to load Sprite: " + spriteName);
        }
    }

}

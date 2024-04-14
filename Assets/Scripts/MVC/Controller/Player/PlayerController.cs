using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Manager<PlayerController>
{
    [SerializeField] private PlayerModel playerModel;
    [SerializeField] private PlayerView playerView;

    private void FixedUpdate() {
        playerModel.OnPlayerMove();
        playerView.SetPlayerPosition(playerModel.currentPos);
    }

    public void SetMap(Map map) {
        playerModel.SetMap(map);
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

    public void OpenPlayerInfoPanel() {
        playerView.OpenPlayerInfoPanel();
    }

}

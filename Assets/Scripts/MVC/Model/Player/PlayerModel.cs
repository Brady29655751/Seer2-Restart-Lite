using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : Module
{
    private Map map;
    [SerializeField] protected const float speed = 8f;
    [SerializeField] private Vector2 canvasSize = new Vector2(960, 540);
    public Action onArriveEvent;
    public Vector2 targetPos { get; private set; }  // Type: Canvas Pos.
    public Vector2 currentPos { get; private set; } // Type: Canvas Pos.
    public Vector2 direction => (targetPos - currentPos); 
    public bool isMoving => (targetPos != currentPos);
    public bool useRobot => Player.instance.gameData.settingsData.useRobotAsPlayer;

    public void SetMap(Map map) {
        this.map = map;
    }

    // MousePos: anchored at bottom-left.
    public void SetDestinationByMousePos(Vector2 destination, Action onArrive) {
        if (!map.IsPathAvailableByMousePos(destination)) {
            return;
        }
        targetPos = map.GetCanvasPixelByMousePos(destination, canvasSize);
        onArriveEvent = onArrive;
    }

    /// <summary>
    /// <paramref name="destination"/> Anchored at bottom-left.
    /// </summary>  
    public void SetDestinationByCanvasPos(Vector2 destination, Action onArrive) {
        targetPos = destination;
        onArriveEvent = onArrive;
    }

    /// <summary>
    /// <paramref name="anchor"/> Bottom-left (0, 0). Top-right (1, 1).
    /// </summary>
    public void SetPlayerPositionByMapPos(Vector2 mapPos, Vector2 anchor) {
        Vector2 canvasPos = map.GetCanvasPixelByMapPos(mapPos, canvasSize, anchor);
        SetPlayerPosition(canvasPos);
    }

    /// <summary>
    /// <paramref name="canvasPos"/> Anchored at bottom-left.
    /// </summary>  
    public void SetPlayerPosition(Vector2 canvasPos) {
        targetPos = currentPos = canvasPos;
    }

    public void OnPlayerMove() {
        if (!isMoving) {
            if (onArriveEvent != null) {
                onArriveEvent?.Invoke();
                onArriveEvent = null;
            }
            return;
        }

        currentPos += speed * direction.normalized;
        if (direction.magnitude <= (speed * 0.6f)) {
            currentPos = targetPos;
        }
    }
}

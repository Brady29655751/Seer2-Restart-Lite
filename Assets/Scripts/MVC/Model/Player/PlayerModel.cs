using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : Module
{
    private Map map;
    private MapNavigator navigator;
    private readonly Queue<Vector2> pathQueue = new Queue<Vector2>();
    private List<Vector2> activePath = new List<Vector2>();
    [SerializeField] protected const float speed = 8f;
    [SerializeField] private Vector2 canvasSize = new Vector2(960, 540);
    public Action onArriveEvent;
    public Vector2 targetPos { get; private set; }  // Type: Canvas Pos.
    public Vector2 currentPos { get; private set; } // Type: Canvas Pos.
    public Vector2 direction => (targetPos - currentPos); 
    public bool isMoving => (targetPos != currentPos) || pathQueue.Count > 0;
    public IReadOnlyList<Vector2> currentPath => activePath;
    public bool useRobot => Player.instance.gameData.settingsData.useRobotAsPlayer;

    public void SetMap(Map map) {
        this.map = map;
        navigator = new MapNavigator(map, canvasSize);
        ClearPath();
    }

    public Vector2 GetCanvasPosByMousePos(Vector2 destination) {
        return map.GetCanvasPixelByMousePos(destination, canvasSize);
    }

    // MousePos: anchored at bottom-left.
    public bool SetDestinationByMousePos(Vector2 destination, Action onArrive) {
        Vector2 canvasDestination = map.GetCanvasPixelByMousePos(destination, canvasSize);
        return SetDestinationByCanvasPos(canvasDestination, onArrive);
    }

    /// <summary>
    /// <paramref name="destination"/> Anchored at bottom-left.
    /// </summary>  
    public bool SetDestinationByCanvasPos(Vector2 destination, Action onArrive) {
        if (navigator == null)
            navigator = new MapNavigator(map, canvasSize);

        if (!navigator.IsTargetReachable(destination))
            return false;

        List<Vector2> path;
        if (navigator.HasLineOfSight(currentPos, destination))
            path = new List<Vector2> { destination };
        else if (!navigator.TryFindPath(currentPos, destination, out path))
            return false;

        SetPath(path);
        onArriveEvent = onArrive;
        return true;
    }

    public bool SetDestinationNearCanvasPos(Vector2 destination, float searchRadius, Action onArrive)
    {
        if (navigator == null)
            navigator = new MapNavigator(map, canvasSize);

        if (!navigator.TryFindNearestReachablePoint(destination, searchRadius, out var reachableDestination))
            return false;

        return SetDestinationByCanvasPos(reachableDestination, onArrive);
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
        ClearPath();
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

        if (targetPos == currentPos && pathQueue.Count > 0)
            targetPos = pathQueue.Dequeue();

        currentPos += speed * direction.normalized;
        if (direction.magnitude <= (speed * 0.6f)) {
            currentPos = targetPos;
        }
    }

    private void SetPath(List<Vector2> path)
    {
        ClearPath();
        if (path == null || path.Count == 0)
            return;

        activePath = new List<Vector2>(path);
        targetPos = path[0];
        for (int i = 1; i < path.Count; i++)
            pathQueue.Enqueue(path[i]);
    }

    private void ClearPath()
    {
        pathQueue.Clear();
        activePath.Clear();
    }
}

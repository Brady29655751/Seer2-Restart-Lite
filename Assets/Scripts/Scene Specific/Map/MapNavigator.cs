using System;
using System.Collections.Generic;
using UnityEngine;

public class MapNavigator
{
    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    private readonly Map map;
    private readonly Vector2 canvasSize;
    private readonly float cellSize;
    private readonly float footprintRadius;
    private readonly int width;
    private readonly int height;
    private readonly bool[] walkable;

    public float footprint => footprintRadius;
    public float gridCellSize => cellSize;

    public MapNavigator(Map map, Vector2 canvasSize, float cellSize = 8f, float footprintRadius = 8f)
    {
        this.map = map;
        this.canvasSize = canvasSize;
        this.cellSize = Mathf.Max(2f, cellSize);
        this.footprintRadius = Mathf.Max(0f, footprintRadius);
        width = Mathf.CeilToInt(canvasSize.x / this.cellSize);
        height = Mathf.CeilToInt(canvasSize.y / this.cellSize);
        walkable = new bool[Mathf.Max(0, width * height)];
        BuildGrid();
    }

    public bool IsTargetReachable(Vector2 canvasPos)
    {
        if (!IsInsideCanvas(canvasPos))
            return false;

        return IsFootprintWalkable(canvasPos);
    }

    public bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        float distance = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(1f, cellSize * 0.5f)));
        for (int i = 0; i <= steps; i++)
        {
            Vector2 sample = Vector2.Lerp(from, to, i / (float)steps);
            if (!IsWalkableAt(sample))
                return false;
        }

        return true;
    }

    public bool TryFindNearestReachablePoint(Vector2 center, float searchRadius, out Vector2 point)
    {
        point = center;
        if (IsTargetReachable(center))
            return true;

        Vector2Int origin = CanvasToCell(center);
        int maxCellRadius = Mathf.CeilToInt(Mathf.Max(0f, searchRadius) / cellSize);
        float bestDistanceSquared = float.PositiveInfinity;
        bool found = false;

        for (int radius = 1; radius <= maxCellRadius; radius++)
        {
            bool foundAtRadius = false;
            for (int y = origin.y - radius; y <= origin.y + radius; y++)
            {
                for (int x = origin.x - radius; x <= origin.x + radius; x++)
                {
                    if (Mathf.Abs(x - origin.x) != radius && Mathf.Abs(y - origin.y) != radius)
                        continue;

                    var cell = new Vector2Int(x, y);
                    if (!IsCellWalkable(cell))
                        continue;

                    Vector2 candidate = CellToCanvas(cell);
                    float distanceSquared = (candidate - center).sqrMagnitude;
                    if (distanceSquared > searchRadius * searchRadius || distanceSquared >= bestDistanceSquared)
                        continue;

                    point = candidate;
                    bestDistanceSquared = distanceSquared;
                    found = true;
                    foundAtRadius = true;
                }
            }

            if (foundAtRadius)
                return true;
        }

        return found;
    }

    public bool TryFindPath(Vector2 from, Vector2 to, out List<Vector2> path)
    {
        path = new List<Vector2>();
        if (!IsTargetReachable(to))
            return false;

        Vector2Int start = CanvasToCell(from);
        Vector2Int goal = CanvasToCell(to);
        if (!IsCellWalkable(start))
            start = FindNearestWalkableCell(start);
        if (!IsCellWalkable(goal))
            goal = FindNearestWalkableCell(goal);
        if (!IsCellWalkable(start) || !IsCellWalkable(goal))
            return false;

        if (start == goal)
        {
            path.Add(to);
            return true;
        }

        if (!TryRunAStar(start, goal, out var cellPath))
            return false;

        path = SmoothPath(from, CellPathToCanvasPath(cellPath, to));
        return path.Count > 0;
    }

    private void BuildGrid()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 center = CellToCanvas(new Vector2Int(x, y));
                walkable[GetIndex(x, y)] = IsFootprintWalkable(center);
            }
        }
    }

    private bool IsWalkableAt(Vector2 canvasPos)
    {
        return IsCellWalkable(CanvasToCell(canvasPos));
    }

    private bool IsFootprintWalkable(Vector2 center)
    {
        if (!IsInsideCanvas(center))
            return false;

        if (!map.IsCanvasPathAvailable(center, canvasSize))
            return false;

        if (map.geometry != null && map.geometry.IntersectsCollisionCircle(center, footprintRadius))
            return false;

        if (footprintRadius <= 0.01f)
            return true;

        for (int i = 0; i < 8; i++)
        {
            float angle = Mathf.PI * 2f * i / 8f;
            Vector2 sample = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * footprintRadius;
            if (!IsInsideCanvas(sample) || !map.IsCanvasPathAvailable(sample, canvasSize))
                return false;
        }

        return true;
    }

    private bool TryRunAStar(Vector2Int start, Vector2Int goal, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        int nodeCount = width * height;
        var cameFrom = new int[nodeCount];
        var gScore = new float[nodeCount];
        var closed = new bool[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            cameFrom[i] = -1;
            gScore[i] = float.PositiveInfinity;
        }

        var open = new List<Vector2Int> { start };
        int startIndex = GetIndex(start.x, start.y);
        int goalIndex = GetIndex(goal.x, goal.y);
        gScore[startIndex] = 0f;

        while (open.Count > 0)
        {
            int currentOpenIndex = GetBestOpenIndex(open, goal, gScore);
            Vector2Int current = open[currentOpenIndex];
            open.RemoveAt(currentOpenIndex);

            int currentIndex = GetIndex(current.x, current.y);
            if (currentIndex == goalIndex)
            {
                path = ReconstructPath(cameFrom, currentIndex);
                return true;
            }

            closed[currentIndex] = true;

            foreach (var offset in NeighborOffsets)
            {
                Vector2Int neighbor = current + offset;
                if (!IsCellWalkable(neighbor))
                    continue;

                if (offset.x != 0 && offset.y != 0 &&
                    (!IsCellWalkable(current + new Vector2Int(offset.x, 0)) ||
                     !IsCellWalkable(current + new Vector2Int(0, offset.y))))
                    continue;

                int neighborIndex = GetIndex(neighbor.x, neighbor.y);
                if (closed[neighborIndex])
                    continue;

                float tentativeScore = gScore[currentIndex] + ((offset.x == 0 || offset.y == 0) ? 1f : 1.4142f);
                if (tentativeScore >= gScore[neighborIndex])
                    continue;

                cameFrom[neighborIndex] = currentIndex;
                gScore[neighborIndex] = tentativeScore;
                if (!open.Contains(neighbor))
                    open.Add(neighbor);
            }
        }

        return false;
    }

    private int GetBestOpenIndex(List<Vector2Int> open, Vector2Int goal, float[] gScore)
    {
        int bestIndex = 0;
        float bestScore = float.PositiveInfinity;
        for (int i = 0; i < open.Count; i++)
        {
            Vector2Int cell = open[i];
            float score = gScore[GetIndex(cell.x, cell.y)] + Heuristic(cell, goal);
            if (score >= bestScore)
                continue;

            bestScore = score;
            bestIndex = i;
        }

        return bestIndex;
    }

    private List<Vector2Int> ReconstructPath(int[] cameFrom, int currentIndex)
    {
        var path = new List<Vector2Int>();
        while (currentIndex >= 0)
        {
            path.Add(IndexToCell(currentIndex));
            currentIndex = cameFrom[currentIndex];
        }

        path.Reverse();
        return path;
    }

    private List<Vector2> CellPathToCanvasPath(List<Vector2Int> cellPath, Vector2 finalTarget)
    {
        var result = new List<Vector2>();
        for (int i = 1; i < cellPath.Count; i++)
            result.Add(CellToCanvas(cellPath[i]));

        if (result.Count == 0 || (result[result.Count - 1] - finalTarget).sqrMagnitude > 1f)
            result.Add(finalTarget);
        else
            result[result.Count - 1] = finalTarget;

        return result;
    }

    private List<Vector2> SmoothPath(Vector2 start, List<Vector2> rawPath)
    {
        if (rawPath.Count <= 2)
            return rawPath;

        var smoothed = new List<Vector2>();
        Vector2 anchorPoint = start;
        int anchor = -1;
        while (anchor < rawPath.Count - 1)
        {
            int next = rawPath.Count - 1;
            for (; next > anchor + 1; next--)
            {
                if (HasLineOfSight(anchorPoint, rawPath[next]))
                    break;
            }

            smoothed.Add(rawPath[next]);
            anchorPoint = rawPath[next];
            anchor = next;
        }

        return smoothed;
    }

    private Vector2Int FindNearestWalkableCell(Vector2Int origin)
    {
        int maxRadius = Mathf.Max(width, height);
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int y = origin.y - radius; y <= origin.y + radius; y++)
            {
                for (int x = origin.x - radius; x <= origin.x + radius; x++)
                {
                    if (Mathf.Abs(x - origin.x) != radius && Mathf.Abs(y - origin.y) != radius)
                        continue;

                    var cell = new Vector2Int(x, y);
                    if (IsCellWalkable(cell))
                        return cell;
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private bool IsCellWalkable(Vector2Int cell)
    {
        return IsCellInside(cell) && walkable[GetIndex(cell.x, cell.y)];
    }

    private bool IsCellInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    private bool IsInsideCanvas(Vector2 point)
    {
        return point.x >= 0f && point.y >= 0f && point.x <= canvasSize.x && point.y <= canvasSize.y;
    }

    private Vector2Int CanvasToCell(Vector2 canvasPos)
    {
        return new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt(canvasPos.x / cellSize), 0, width - 1),
            Mathf.Clamp(Mathf.FloorToInt(canvasPos.y / cellSize), 0, height - 1));
    }

    private Vector2 CellToCanvas(Vector2Int cell)
    {
        return new Vector2(
            Mathf.Clamp((cell.x + 0.5f) * cellSize, 0f, canvasSize.x),
            Mathf.Clamp((cell.y + 0.5f) * cellSize, 0f, canvasSize.y));
    }

    private Vector2Int IndexToCell(int index)
    {
        return new Vector2Int(index % width, index / width);
    }

    private int GetIndex(int x, int y)
    {
        return y * width + x;
    }

    private float Heuristic(Vector2Int lhs, Vector2Int rhs)
    {
        int dx = Mathf.Abs(lhs.x - rhs.x);
        int dy = Mathf.Abs(lhs.y - rhs.y);
        return Math.Max(dx, dy);
    }
}

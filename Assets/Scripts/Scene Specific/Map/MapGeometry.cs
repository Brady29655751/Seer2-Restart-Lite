using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class MapGeometry
{
    [XmlElement("mask")] public List<MapPolygon> masks = new List<MapPolygon>();
    [XmlElement("collision")] public List<MapPolygon> collisions = new List<MapPolygon>();

    public IEnumerable<MapPolygon> ValidMasks => (masks ?? new List<MapPolygon>()).Where(x => x.IsValid);
    public IEnumerable<MapPolygon> ValidCollisions => (collisions ?? new List<MapPolygon>()).Where(x => x.IsValid);

    public void EnsureLists()
    {
        masks ??= new List<MapPolygon>();
        collisions ??= new List<MapPolygon>();
    }

    public bool TryGetFirstCollisionHit(Vector2 from, Vector2 to, out float hitT)
    {
        hitT = 1f;
        bool hasHit = false;

        foreach (var polygon in ValidCollisions)
        {
            if (!polygon.TryGetMovementHit(from, to, out var polygonHitT))
                continue;

            if (polygonHitT < hitT)
            {
                hitT = polygonHitT;
                hasHit = true;
            }
        }

        return hasHit;
    }

    public bool ContainsCollisionPoint(Vector2 point)
    {
        foreach (var polygon in ValidCollisions)
        {
            if (polygon.ContainsPoint(point))
                return true;
        }

        return false;
    }
}

public class MapPolygon
{
    [XmlAttribute("id")] public int id;
    [XmlAttribute("name")] public string name;
    [XmlAttribute("points")] public string pointData;

    [XmlIgnore] public List<Vector2> points
    {
        get => MapGeometryUtility.ParsePoints(pointData);
        set => pointData = MapGeometryUtility.FormatPoints(value);
    }

    [XmlIgnore] public bool IsValid => points.Count >= 3;

    public bool ContainsPoint(Vector2 point)
    {
        return MapGeometryUtility.ContainsPoint(points, point);
    }

    public bool TryGetMovementHit(Vector2 from, Vector2 to, out float hitT)
    {
        hitT = 1f;
        var polygonPoints = points;
        if (polygonPoints.Count < 3)
            return false;

        bool startInside = MapGeometryUtility.ContainsPoint(polygonPoints, from);
        bool endInside = MapGeometryUtility.ContainsPoint(polygonPoints, to);

        if (startInside)
            return false;

        bool hasHit = false;
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            Vector2 edgeStart = polygonPoints[i];
            Vector2 edgeEnd = polygonPoints[(i + 1) % polygonPoints.Count];
            if (!MapGeometryUtility.TryGetSegmentIntersectionT(from, to, edgeStart, edgeEnd, out var t))
                continue;

            if (t < hitT)
            {
                hitT = t;
                hasHit = true;
            }
        }

        if (endInside && !hasHit)
        {
            hitT = 0f;
            return true;
        }

        return hasHit;
    }
}

public static class MapGeometryUtility
{
    private const float Epsilon = 0.001f;

    public static List<Vector2> ParsePoints(string pointData)
    {
        var result = new List<Vector2>();
        if (string.IsNullOrWhiteSpace(pointData))
            return result;

        string[] entries = pointData.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string entry in entries)
        {
            string[] coords = entry.Trim().Split(',');
            if (coords.Length != 2)
                continue;

            if (!TryParseFloat(coords[0], out var x) || !TryParseFloat(coords[1], out var y))
                continue;

            result.Add(new Vector2(x, y));
        }

        return result;
    }

    public static string FormatPoints(IEnumerable<Vector2> points)
    {
        if (points == null)
            return string.Empty;

        return points.Select(point =>
            FormatFloat(point.x) + "," + FormatFloat(point.y)).ConcatToString(";");
    }

    public static bool ContainsPoint(IReadOnlyList<Vector2> polygon, Vector2 point)
    {
        if (polygon == null || polygon.Count < 3)
            return false;

        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];
            if (IsPointOnSegment(point, a, b))
                return true;

            bool crosses = (a.y > point.y) != (b.y > point.y);
            if (crosses)
            {
                float x = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < x)
                    inside = !inside;
            }
        }

        return inside;
    }

    public static bool TryGetSegmentIntersectionT(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out float t)
    {
        t = 0f;
        Vector2 r = b - a;
        Vector2 s = d - c;
        float denominator = Cross(r, s);
        Vector2 cMinusA = c - a;

        if (Mathf.Abs(denominator) < Epsilon)
        {
            if (Mathf.Abs(Cross(cMinusA, r)) >= Epsilon)
                return false;

            float lengthSquared = Vector2.Dot(r, r);
            if (lengthSquared < Epsilon)
                return false;

            float t0 = Vector2.Dot(c - a, r) / lengthSquared;
            float t1 = Vector2.Dot(d - a, r) / lengthSquared;
            float min = Mathf.Max(0f, Mathf.Min(t0, t1));
            float max = Mathf.Min(1f, Mathf.Max(t0, t1));
            if (min > max)
                return false;

            t = min;
            return true;
        }

        float segmentT = Cross(cMinusA, s) / denominator;
        float segmentU = Cross(cMinusA, r) / denominator;
        if (!IsWithin01(segmentT) || !IsWithin01(segmentU))
            return false;

        t = Mathf.Clamp01(segmentT);
        return true;
    }

    public static List<int> Triangulate(IReadOnlyList<Vector2> polygon)
    {
        var indices = new List<int>();
        if (polygon == null || polygon.Count < 3)
            return indices;

        var remaining = Enumerable.Range(0, polygon.Count).ToList();
        if (SignedArea(polygon) < 0f)
            remaining.Reverse();

        int guard = polygon.Count * polygon.Count;
        while (remaining.Count > 3 && guard-- > 0)
        {
            bool clipped = false;
            for (int i = 0; i < remaining.Count; i++)
            {
                int prevIndex = remaining[(i - 1 + remaining.Count) % remaining.Count];
                int currentIndex = remaining[i];
                int nextIndex = remaining[(i + 1) % remaining.Count];

                Vector2 prev = polygon[prevIndex];
                Vector2 current = polygon[currentIndex];
                Vector2 next = polygon[nextIndex];
                if (Cross(current - prev, next - current) <= Epsilon)
                    continue;

                bool containsPoint = false;
                for (int j = 0; j < remaining.Count; j++)
                {
                    int testIndex = remaining[j];
                    if (testIndex == prevIndex || testIndex == currentIndex || testIndex == nextIndex)
                        continue;

                    if (IsPointInTriangle(polygon[testIndex], prev, current, next))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (containsPoint)
                    continue;

                indices.Add(prevIndex);
                indices.Add(currentIndex);
                indices.Add(nextIndex);
                remaining.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped)
                break;
        }

        if (remaining.Count == 3)
        {
            indices.Add(remaining[0]);
            indices.Add(remaining[1]);
            indices.Add(remaining[2]);
        }

        return indices;
    }

    private static bool TryParseFloat(string value, out float result)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;

        return float.TryParse(value, out result);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static bool IsWithin01(float value)
    {
        return value >= -Epsilon && value <= 1f + Epsilon;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static float SignedArea(IReadOnlyList<Vector2> polygon)
    {
        float area = 0f;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];
            area += current.x * next.y - next.x * current.y;
        }

        return area * 0.5f;
    }

    private static bool IsPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        if (Mathf.Abs(Cross(point - a, b - a)) > Epsilon)
            return false;

        return point.x.IsWithin(Mathf.Min(a.x, b.x) - Epsilon, Mathf.Max(a.x, b.x) + Epsilon) &&
               point.y.IsWithin(Mathf.Min(a.y, b.y) - Epsilon, Mathf.Max(a.y, b.y) + Epsilon);
    }

    private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area1 = Cross(b - a, point - a);
        float area2 = Cross(c - b, point - b);
        float area3 = Cross(a - c, point - c);
        bool hasNegative = area1 < -Epsilon || area2 < -Epsilon || area3 < -Epsilon;
        bool hasPositive = area1 > Epsilon || area2 > Epsilon || area3 > Epsilon;
        return !(hasNegative && hasPositive);
    }
}

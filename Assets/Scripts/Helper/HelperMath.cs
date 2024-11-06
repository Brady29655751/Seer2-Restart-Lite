using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {

public static class MathHelper {

    /// <summary>
    /// Check if a number is in the range (Max exclusive).
    /// </summary>
    public static bool IsInRange(this int num, int min, int max) {
        return ((num >= min) && (num < max));
    }

    /// <summary>
    /// Check if a number is in the range (Max inclusive).
    /// </summary>
    public static bool IsWithin(this int num, int min, int max) {
        if (min > max) {
            var tmp = min;  min = max;  max = tmp;
        }
        return ((num >= min) && (num <= max));
    }

    /// <summary>
    /// Check if a number is in the range (Max inclusive).
    /// </summary>
    public static bool IsWithin(this float num, float min, float max) {
        if (min > max) {
            var tmp = min;  min = max;  max = tmp;
        }
        return ((num >= min) && (num <= max));
    }
}

public static class Vector {

    /// <summary>
    /// <paramref name="anchor"/> Bottom-left (0, 0). Top-right (1, 1).
    /// </summary>
    public static Vector2 Anchor(this Vector2 pos, Vector2 refSize, Vector2 anchor) {
        Vector2 clampedAnchor = new Vector2(Mathf.Clamp01(anchor.x), Mathf.Clamp01(anchor.y));
        float newX = refSize.x * (clampedAnchor.x) + pos.x * (1 - 2 * clampedAnchor.x);
        float newY = refSize.y * (clampedAnchor.y) + pos.y * (1 - 2 * clampedAnchor.y);
        return new Vector2(newX, newY);
    }

    public static Vector2 Sign(this Vector2 v) {
        return new Vector2(Mathf.Sign(v.x), Mathf.Sign(v.y));
    }

    public static Vector2 Ratio(Vector2 lhs, Vector2 rhs) {
        return new Vector2(lhs.x / rhs.x, lhs.y / rhs.y);
    }

    public static Vector2 Scale(Vector2 lhs, Vector2 rhs) {
        return Vector2.Scale(lhs, rhs);
    }

    public static Vector2Int RoundToInt(this Vector2 v) {
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }

    public static Vector2Int GetCorrespondingPixel(this Vector2 point, Vector2 inputSize, Vector2 outputSize) {
        Vector2 scaleFactor = Ratio(outputSize, inputSize);
        Vector2 closest = Scale(scaleFactor, point);
        return closest.RoundToInt();
    }

}

}

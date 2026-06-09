using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPathPreviewView : MonoBehaviour
{
    [Tooltip("Length of each route dash in canvas pixels.")]
    [SerializeField] private float dashLength = 14f;
    [Tooltip("Gap between route dashes in canvas pixels.")]
    [SerializeField] private float dashGap = 8f;
    [Tooltip("Width of each route dash in canvas pixels.")]
    [SerializeField] private float lineWidth = 3f;
    [Tooltip("Tint and opacity of the route preview.")]
    [SerializeField] private Color lineColor = new Color(0.25f, 0.95f, 1f, 0.46f);

    private readonly List<Image> dashImages = new List<Image>();
    private Sprite dashSprite;
    private Texture2D dashTexture;

    public void SetPath(Vector2 start, IReadOnlyList<Vector2> path)
    {
        if (path == null || path.Count <= 1)
        {
            Clear();
            return;
        }

        EnsureSprite();

        int dashIndex = 0;
        Vector2 from = start;
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 to = path[i];
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                from = to;
                continue;
            }

            Vector2 direction = delta / length;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            for (float offset = 0f; offset < length; offset += dashLength + dashGap)
            {
                float currentDashLength = Mathf.Min(dashLength, length - offset);
                if (currentDashLength <= 1f)
                    continue;

                Image dash = GetDashImage(dashIndex++);
                dash.gameObject.SetActive(true);
                dash.sprite = dashSprite;
                dash.color = lineColor;

                var rect = dash.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = from + direction * (offset + currentDashLength * 0.5f);
                rect.sizeDelta = new Vector2(currentDashLength, lineWidth);
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.localScale = Vector3.one;
            }

            from = to;
        }

        for (int i = dashIndex; i < dashImages.Count; i++)
            dashImages[i].gameObject.SetActive(false);
    }

    public void Clear()
    {
        foreach (var image in dashImages)
            image.gameObject.SetActive(false);
    }

    private Image GetDashImage(int index)
    {
        while (dashImages.Count <= index)
        {
            var dashObject = new GameObject("Path Dash", typeof(RectTransform), typeof(Image));
            var dashRect = dashObject.GetComponent<RectTransform>();
            dashRect.SetParent(transform, false);

            Image image = dashObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            dashImages.Add(image);
        }

        return dashImages[index];
    }

    private void EnsureSprite()
    {
        if (dashSprite != null)
            return;

        dashTexture = CreateDashTexture(32, 8);
        dashTexture.Apply(false, false);
        dashSprite = Sprite.Create(dashTexture, new Rect(0f, 0f, dashTexture.width, dashTexture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Texture2D CreateDashTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color32[width * height];
        float centerY = (height - 1) * 0.5f;
        float halfHeight = height * 0.42f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float edgeFadeX = Mathf.Min(x, width - 1 - x) / 4f;
                float edgeFadeY = 1f - Mathf.Clamp01((Mathf.Abs(y - centerY) - halfHeight + 1f) / 2f);
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(Mathf.Min(edgeFadeX, edgeFadeY)) * 255f);
                pixels[y * width + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private void OnDestroy()
    {
        if (dashSprite != null)
            Destroy(dashSprite);

        if (dashTexture != null)
            Destroy(dashTexture);
    }
}

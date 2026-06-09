using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapForegroundMaskGraphic : Image
{
    private Sprite generatedSprite;
    private Texture2D generatedTexture;

    public int copiedPixelCount { get; private set; }
    public bool hasGeneratedMask => generatedSprite != null;

    public void SetPolygons(Sprite foregroundSprite, IEnumerable<MapPolygon> polygons, Vector2 referenceSize)
    {
        ReleaseGeneratedAssets();
        raycastTarget = false;
        type = Type.Simple;
        preserveAspect = false;
        copiedPixelCount = 0;

        var validPolygons = polygons?.Where(polygon => polygon != null && polygon.IsValid).ToList();
        if (foregroundSprite == null || foregroundSprite.texture == null || validPolygons == null || validPolygons.Count == 0)
        {
            sprite = null;
            return;
        }

        generatedTexture = CreateMaskTexture(foregroundSprite, validPolygons, referenceSize);
        if (generatedTexture == null || copiedPixelCount == 0)
        {
            sprite = null;
            return;
        }

        generatedTexture.name = foregroundSprite.name + "_foreground_mask_texture";
        generatedTexture.Apply(false, false);
        generatedSprite = Sprite.Create(
            generatedTexture,
            new Rect(0f, 0f, generatedTexture.width, generatedTexture.height),
            Vector2.zero,
            foregroundSprite.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
        generatedSprite.name = foregroundSprite.name + "_foreground_mask";
        sprite = generatedSprite;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    protected override void OnDestroy()
    {
        ReleaseGeneratedAssets();
        base.OnDestroy();
    }

    private Texture2D CreateMaskTexture(Sprite foregroundSprite, List<MapPolygon> polygons, Vector2 referenceSize)
    {
        Texture2D sourceTexture = foregroundSprite.texture;
        Rect sourceRect = foregroundSprite.textureRect;
        int width = Mathf.RoundToInt(sourceRect.width);
        int height = Mathf.RoundToInt(sourceRect.height);
        if (width <= 0 || height <= 0)
            return null;

        Color32[] sourcePixels;
        try
        {
            sourcePixels = sourceTexture.GetPixels32();
        }
        catch (UnityException exception)
        {
            Debug.LogWarning("Map foreground mask could not read the background texture: " + exception.Message);
            return null;
        }

        var outputPixels = new Color32[width * height];
        int sourceXOffset = Mathf.RoundToInt(sourceRect.x);
        int sourceYOffset = Mathf.RoundToInt(sourceRect.y);
        Vector2 safeReferenceSize = new Vector2(
            referenceSize.x <= 0f ? 960f : referenceSize.x,
            referenceSize.y <= 0f ? 540f : referenceSize.y);

        foreach (var polygon in polygons)
        {
            var points = polygon.points;
            RectInt pixelBounds = GetPixelBounds(points, safeReferenceSize, width, height);
            for (int y = pixelBounds.yMin; y < pixelBounds.yMax; y++)
            {
                float referenceY = (y + 0.5f) / height * safeReferenceSize.y;
                for (int x = pixelBounds.xMin; x < pixelBounds.xMax; x++)
                {
                    float referenceX = (x + 0.5f) / width * safeReferenceSize.x;
                    if (!MapGeometryUtility.ContainsPoint(points, new Vector2(referenceX, referenceY)))
                        continue;

                    int sourceX = Mathf.Clamp(sourceXOffset + x, 0, sourceTexture.width - 1);
                    int sourceY = Mathf.Clamp(sourceYOffset + y, 0, sourceTexture.height - 1);
                    outputPixels[y * width + x] = sourcePixels[sourceY * sourceTexture.width + sourceX];
                    copiedPixelCount++;
                }
            }
        }

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels32(outputPixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = sourceTexture.filterMode;
        return texture;
    }

    private RectInt GetPixelBounds(List<Vector2> points, Vector2 referenceSize, int textureWidth, int textureHeight)
    {
        float minX = points.Min(point => point.x);
        float maxX = points.Max(point => point.x);
        float minY = points.Min(point => point.y);
        float maxY = points.Max(point => point.y);

        int xMin = Mathf.Clamp(Mathf.FloorToInt(minX / referenceSize.x * textureWidth), 0, textureWidth);
        int xMax = Mathf.Clamp(Mathf.CeilToInt(maxX / referenceSize.x * textureWidth), 0, textureWidth);
        int yMin = Mathf.Clamp(Mathf.FloorToInt(minY / referenceSize.y * textureHeight), 0, textureHeight);
        int yMax = Mathf.Clamp(Mathf.CeilToInt(maxY / referenceSize.y * textureHeight), 0, textureHeight);
        return new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
    }

    private void ReleaseGeneratedAssets()
    {
        if (generatedSprite != null)
            DestroyGeneratedAsset(generatedSprite);

        if (generatedTexture != null)
            DestroyGeneratedAsset(generatedTexture);

        generatedSprite = null;
        generatedTexture = null;
        copiedPixelCount = 0;
    }

    private void DestroyGeneratedAsset(Object asset)
    {
        if (asset == null)
            return;

        if (Application.isPlaying)
            Destroy(asset);
        else
            DestroyImmediate(asset);
    }
}

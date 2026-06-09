using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapClickFeedbackView : Image
{
    [SerializeField] private float duration = 0.65f;
    [SerializeField] private float startScale = 0.45f;
    [SerializeField] private float endScale = 1.15f;
    [SerializeField] private Color feedbackColor = new Color(0.25f, 0.95f, 1f, 0.9f);

    private RectTransform rectTransformCache;
    private Coroutine playCoroutine;
    private Sprite generatedSprite;
    private Texture2D generatedTexture;

    protected override void Awake()
    {
        base.Awake();
        rectTransformCache = transform as RectTransform;
        raycastTarget = false;
        type = Type.Simple;
        preserveAspect = true;
        color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0f);
        EnsureSprite();
    }

    public void Play(Vector2 canvasPos)
    {
        if (rectTransformCache == null)
            rectTransformCache = transform as RectTransform;

        rectTransformCache.anchoredPosition = canvasPos;
        rectTransformCache.localScale = Vector3.one * startScale;
        gameObject.SetActive(true);
        EnsureSprite();

        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        playCoroutine = StartCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        float elapsed = 0f;
        Color baseColor = feedbackColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            rectTransformCache.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, progress);
            float alpha = Mathf.Lerp(feedbackColor.a, 0f, SmoothEase(progress));
            color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        rectTransformCache.localScale = Vector3.one;
        playCoroutine = null;
        gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        ReleaseGeneratedAssets();
        base.OnDestroy();
    }

    private void EnsureSprite()
    {
        if (sprite != null)
            return;

        generatedTexture = CreateCircleTexture(96);
        generatedTexture.name = "map_click_feedback_texture";
        generatedTexture.Apply(false, false);
        generatedSprite = Sprite.Create(
            generatedTexture,
            new Rect(0f, 0f, generatedTexture.width, generatedTexture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        generatedSprite.name = "map_click_feedback";
        sprite = generatedSprite;
    }

    private Texture2D CreateCircleTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float center = (size - 1) * 0.5f;
        float maxRadius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / maxRadius;
                float mainRing = SmoothBand(distance, 0.58f, 0.035f, 0.045f);
                float outerRing = SmoothBand(distance, 0.82f, 0.025f, 0.04f) * 0.45f;
                float centerGlow = (1f - SmoothStep(0.1f, 0.7f, distance)) * 0.08f;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(Mathf.Max(mainRing, Mathf.Max(outerRing, centerGlow))) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private float SmoothBand(float value, float center, float width, float feather)
    {
        float distance = Mathf.Abs(value - center);
        return 1f - SmoothStep(width, width + feather, distance);
    }

    private float SmoothStep(float edge0, float edge1, float value)
    {
        float t = Mathf.Clamp01((value - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    private float SmoothEase(float value)
    {
        float t = Mathf.Clamp01(value);
        return t * t * (3f - 2f * t);
    }

    private void ReleaseGeneratedAssets()
    {
        if (generatedSprite != null)
            Destroy(generatedSprite);

        if (generatedTexture != null)
            Destroy(generatedTexture);

        generatedSprite = null;
        generatedTexture = null;
    }
}

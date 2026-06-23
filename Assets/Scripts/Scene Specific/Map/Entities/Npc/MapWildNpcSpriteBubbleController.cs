using UnityEngine;
using UnityEngine.UI;

public class MapWildNpcSpriteBubbleController : MonoBehaviour
{
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text bubbleText;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private int maxTextLength = 30;
    [SerializeField] private float fadeTime = 0.18f;
    [SerializeField] private int fontSize = 17;
    [SerializeField] private float minWidth = 50f;
    [SerializeField] private float maxWidth = 200f;
    [SerializeField] private float minHeight = 72f;
    [SerializeField] private float paddingX = 15f;
    [SerializeField] private float paddingY = 20f;
    [SerializeField] private float tailReserve = 15f;
    [SerializeField] private TextAnchor textAnchor = TextAnchor.MiddleLeft;
    [SerializeField] private Vector2 imageMaxSize = new Vector2(120f, 120f);

    private float remainingTime;
    private float totalDuration;

    private void Awake()
    {
        CacheReferences();
    }

    public void Init()
    {
        CacheReferences();
        ApplyTextStyle();
        HideImmediate();
    }

    public void ShowText(string content, float duration)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        CacheReferences();
        if (bubbleRect == null || canvasGroup == null || bubbleText == null)
            return;

        string text = TrimText(content.Trim());
        SetContentActive(showText: true);
        bubbleText.text = text;
        ApplyTextStyle();
        Vector2 bubbleSize = GetBubbleSize(text);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bubbleSize.x);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleSize.y);
        ApplyTextPadding();

        BeginShow(duration);
    }

    public void ShowImage(Sprite sprite, float duration)
    {
        if (sprite == null)
            return;

        CacheReferences();
        if (bubbleRect == null || canvasGroup == null || bubbleImage == null)
            return;

        SetContentActive(showText: false);
        bubbleImage.sprite = sprite;
        bubbleImage.preserveAspect = true;

        Vector2 imageSize = GetImageSize(sprite);
        float safePaddingX = Mathf.Max(0f, paddingX);
        float safePaddingY = Mathf.Max(0f, paddingY);
        float width = Mathf.Max(minWidth, imageSize.x + safePaddingX * 2f);
        float height = Mathf.Max(minHeight, imageSize.y + safePaddingY * 2f + Mathf.Max(0f, tailReserve));
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        ApplyContentPadding(bubbleImage.rectTransform);

        BeginShow(duration);
    }

    public void HideImmediate()
    {
        remainingTime = 0f;
        totalDuration = 0f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (bubbleRect != null)
            bubbleRect.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (bubbleRect == null || !bubbleRect.gameObject.activeSelf)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            HideImmediate();
            return;
        }

        float fade = Mathf.Max(0.01f, fadeTime);
        if (remainingTime < fade)
            canvasGroup.alpha = Mathf.Clamp01(remainingTime / fade);
        else if (totalDuration - remainingTime < fade)
            canvasGroup.alpha = Mathf.Clamp01((totalDuration - remainingTime) / fade);
        else
            canvasGroup.alpha = 1f;
    }

    private void CacheReferences()
    {
        if (bubbleRect == null)
            bubbleRect = transform as RectTransform;
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (bubbleText == null)
            bubbleText = GetComponentInChildren<Text>(true);
        if (bubbleImage == null)
        {
            Transform imageTransform = transform.Find("Image Content");
            if (imageTransform != null)
                bubbleImage = imageTransform.GetComponent<Image>();
        }
    }

    private void ApplyTextStyle()
    {
        if (bubbleText == null)
            return;

        bubbleText.alignment = textAnchor;
        bubbleText.fontSize = fontSize;
        bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bubbleText.verticalOverflow = VerticalWrapMode.Truncate;
        bubbleText.raycastTarget = false;
        bubbleText.supportRichText = false;

        if (bubbleText.font == null)
            bubbleText.font = GetFallbackFont();
    }

    private Vector2 GetBubbleSize(string text)
    {
        float safeMinWidth = Mathf.Max(1f, minWidth);
        float safeMaxWidth = Mathf.Max(safeMinWidth, maxWidth);
        float safePaddingX = Mathf.Max(0f, paddingX);
        float safePaddingY = Mathf.Max(0f, paddingY);
        float safeTailReserve = Mathf.Max(0f, tailReserve);
        float maxTextWidth = Mathf.Max(1f, safeMaxWidth - safePaddingX * 2f);
        float preferredWidth = bubbleText.cachedTextGeneratorForLayout.GetPreferredWidth(
            text,
            bubbleText.GetGenerationSettings(new Vector2(maxTextWidth, 0f))) / bubbleText.pixelsPerUnit;
        float width = Mathf.Clamp(preferredWidth + safePaddingX * 2f, safeMinWidth, safeMaxWidth);

        float contentWidth = Mathf.Max(1f, width - safePaddingX * 2f);
        float preferredHeight = bubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(
            text,
            bubbleText.GetGenerationSettings(new Vector2(contentWidth, 0f))) / bubbleText.pixelsPerUnit;
        float height = Mathf.Max(minHeight, preferredHeight + safePaddingY * 2f + safeTailReserve);
        return new Vector2(width, height);
    }

    private void ApplyTextPadding()
    {
        ApplyContentPadding(bubbleText.rectTransform);
    }

    private void ApplyContentPadding(RectTransform contentRect)
    {
        if (contentRect == null)
            return;

        float safePaddingX = Mathf.Max(0f, paddingX);
        float safePaddingY = Mathf.Max(0f, paddingY);
        float safeTailReserve = Mathf.Max(0f, tailReserve);
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(safePaddingX, safePaddingY + safeTailReserve);
        contentRect.offsetMax = new Vector2(-safePaddingX, -safePaddingY);
    }

    private Font GetFallbackFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private Vector2 GetImageSize(Sprite sprite)
    {
        Vector2 nativeSize = sprite.rect.size;
        Vector2 safeMaxSize = new Vector2(
            Mathf.Max(1f, imageMaxSize.x),
            Mathf.Max(1f, imageMaxSize.y));
        float scale = Mathf.Min(1f, safeMaxSize.x / nativeSize.x, safeMaxSize.y / nativeSize.y);
        return nativeSize * scale;
    }

    private void SetContentActive(bool showText)
    {
        if (bubbleText != null)
            bubbleText.gameObject.SetActive(showText);
        if (bubbleImage != null)
            bubbleImage.gameObject.SetActive(!showText);
    }

    private void BeginShow(float duration)
    {
        remainingTime = Mathf.Max(0.2f, duration);
        totalDuration = remainingTime;
        bubbleRect.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    private string TrimText(string text)
    {
        if (text.Length <= maxTextLength)
            return text;

        return text.Substring(0, maxTextLength);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MapWildNpcSpriteBubbleController : MonoBehaviour
{
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text bubbleText;
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

    public void Show(string content, float duration)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        CacheReferences();
        if (bubbleRect == null || canvasGroup == null || bubbleText == null)
            return;

        string text = TrimText(content.Trim());
        bubbleText.text = text;
        ApplyTextStyle();
        Vector2 bubbleSize = GetBubbleSize(text);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bubbleSize.x);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleSize.y);
        ApplyTextPadding();

        remainingTime = Mathf.Max(0.2f, duration);
        totalDuration = remainingTime;
        bubbleRect.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
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
        var textRect = bubbleText.rectTransform;
        float safePaddingX = Mathf.Max(0f, paddingX);
        float safePaddingY = Mathf.Max(0f, paddingY);
        float safeTailReserve = Mathf.Max(0f, tailReserve);
        textRect.offsetMin = new Vector2(safePaddingX, safePaddingY + safeTailReserve);
        textRect.offsetMax = new Vector2(-safePaddingX, -safePaddingY);
    }

    private Font GetFallbackFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private string TrimText(string text)
    {
        if (text.Length <= maxTextLength)
            return text;

        return text.Substring(0, maxTextLength);
    }
}

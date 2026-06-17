using UnityEngine;
using UnityEngine.UI;

public class MapWildNpcSpriteBubbleController : MonoBehaviour
{
    private const int MaxTextLength = 30;
    private const float FadeTime = 0.18f;
    private const int BubbleFontSize = 17;
    private const float MinBubbleWidth = 50f;
    private const float MaxBubbleWidth = 200f;
    private const float MinBubbleHeight = 72f;
    private const float BubblePaddingX = 15f;
    private const float BubblePaddingY = 20f;
    private const float BubbleTailHeight = 15f;
    private const float BubbleBottomOffset = 7f;
    private const string BubbleSpritePath = "Sprites/UI/WildNpcBubbleIntegrated";
    private const TextAnchor BubbleTextAnchor = TextAnchor.MiddleLeft; // MiddleLeft左对齐，MiddleCenter居中对齐，MiddleRight右对齐

    private NpcInfo info;
    private RectTransform bubbleRect;
    private CanvasGroup canvasGroup;
    private Text bubbleText;
    private Image bubbleImage;
    private float remainingTime;
    private float totalDuration;

    public void Init(NpcInfo info)
    {
        this.info = info;
        EnsureView();
        HideImmediate();
    }

    public void Show(string content, float duration)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        EnsureView();
        string text = TrimText(content.Trim());
        bubbleText.text = text;
        bubbleText.alignment = BubbleTextAnchor;
        Vector2 bubbleSize = GetBubbleSize(text);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bubbleSize.x);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleSize.y);
        bubbleRect.anchoredPosition = GetBubblePosition();
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

        if (remainingTime < FadeTime)
            canvasGroup.alpha = Mathf.Clamp01(remainingTime / FadeTime);
        else if (totalDuration - remainingTime < FadeTime)
            canvasGroup.alpha = Mathf.Clamp01((totalDuration - remainingTime) / FadeTime);
        else
            canvasGroup.alpha = 1f;
    }

    private void EnsureView()
    {
        if (bubbleRect != null)
            return;

        var bubbleObject = new GameObject("Wild Npc Bubble", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        bubbleRect = bubbleObject.GetComponent<RectTransform>();
        bubbleRect.SetParent(transform, false);
        bubbleRect.anchorMin = new Vector2(0.5f, 0.5f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        bubbleRect.localScale = Vector3.one;

        canvasGroup = bubbleObject.GetComponent<CanvasGroup>();
        bubbleImage = bubbleObject.GetComponent<Image>();
        bubbleImage.sprite = Resources.Load<Sprite>(BubbleSpritePath);
        bubbleImage.type = Image.Type.Simple;
        bubbleImage.preserveAspect = false;
        bubbleImage.color = Color.white;
        bubbleImage.raycastTarget = false;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(bubbleRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);

        bubbleText = textObject.GetComponent<Text>();
        bubbleText.alignment = BubbleTextAnchor;
        bubbleText.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        bubbleText.fontSize = BubbleFontSize;
        bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bubbleText.verticalOverflow = VerticalWrapMode.Truncate;
        bubbleText.raycastTarget = false;
        bubbleText.supportRichText = false;
        bubbleText.font = GetBubbleFont();

        var shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(1f, 1f, 1f, 0.4f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    private Font GetBubbleFont()
    {
        var nameText = transform.Find("View/Name")?.GetComponent<Text>();
        if (nameText?.font != null)
            return nameText.font;

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private Vector2 GetBubblePosition()
    {
        float npcHeight = Mathf.Max(40f, info?.size.y ?? 40f);
        return new Vector2(0f, npcHeight * 0.5f + BubbleBottomOffset);
    }

    private Vector2 GetBubbleSize(string text)
    {
        float maxTextWidth = MaxBubbleWidth - BubblePaddingX * 2f;
        float preferredWidth = bubbleText.cachedTextGeneratorForLayout.GetPreferredWidth(
            text,
            bubbleText.GetGenerationSettings(new Vector2(maxTextWidth, 0f))) / bubbleText.pixelsPerUnit;
        float width = Mathf.Clamp(preferredWidth + BubblePaddingX * 2f, MinBubbleWidth, MaxBubbleWidth);

        float contentWidth = Mathf.Max(40f, width - BubblePaddingX * 2f);
        float preferredHeight = bubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(
            text,
            bubbleText.GetGenerationSettings(new Vector2(contentWidth, 0f))) / bubbleText.pixelsPerUnit;
        float height = Mathf.Max(MinBubbleHeight, preferredHeight + BubblePaddingY * 2f + BubbleTailHeight);
        return new Vector2(width, height);
    }

    private void ApplyTextPadding()
    {
        var textRect = bubbleText.rectTransform;
        textRect.offsetMin = new Vector2(BubblePaddingX, BubblePaddingY + BubbleTailHeight);
        textRect.offsetMax = new Vector2(-BubblePaddingX, -BubblePaddingY);
    }

    private string TrimText(string text)
    {
        if (text.Length <= MaxTextLength)
            return text;

        return text.Substring(0, MaxTextLength);
    }
}

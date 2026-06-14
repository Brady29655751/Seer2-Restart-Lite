using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleToolbarButtonHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rect;
    private Button button;
    private Vector3 baseScale;
    private Vector2 basePosition;
    private float hoverScale = 1.06f;
    private float hoverLerpSpeed = 12f;
    private bool hoverActive;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        if (rect == null)
            return;

        baseScale = rect.localScale;
        basePosition = rect.anchoredPosition;
    }

    private void Update()
    {
        if (rect == null)
            return;

        var scale = hoverActive ? hoverScale : 1f;
        var targetScale = baseScale * scale;
        var targetPosition = basePosition + GetPivotCompensation(scale);
        var progress = 1f - Mathf.Exp(-hoverLerpSpeed * Time.unscaledDeltaTime);

        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, progress);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, progress);
    }

    public void SetFeedback(float scale, float lerpSpeed)
    {
        hoverScale = scale;
        hoverLerpSpeed = lerpSpeed;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if ((button != null) && !button.interactable)
            return;

        hoverActive = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverActive = false;
    }

    private void OnDisable()
    {
        hoverActive = false;

        if (rect == null)
            return;

        rect.localScale = baseScale;
        rect.anchoredPosition = basePosition;
    }

    private Vector2 GetPivotCompensation(float scale)
    {
        if (rect == null)
            return Vector2.zero;

        var scaleDelta = scale - 1f;
        return new Vector2(
            (rect.pivot.x - 0.5f) * rect.rect.width * scaleDelta,
            (rect.pivot.y - 0.5f) * rect.rect.height * scaleDelta
        );
    }
}

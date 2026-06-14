using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverScaleFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float hoverLerpSpeed = 12f;
    [SerializeField] private bool scaleFromCenter = true;

    private Button button;
    private Vector3 baseScale;
    private bool hoverActive;

    private RectTransform target => targetRect != null ? targetRect : transform as RectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();
        CacheBaseTransform();
    }

    private void Update()
    {
        var rect = target;
        if (rect == null)
            return;

        var scale = hoverActive ? hoverScale : 1f;
        var targetScale = baseScale * scale;
        var progress = 1f - Mathf.Exp(-hoverLerpSpeed * Time.unscaledDeltaTime);

        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, progress);
    }

    public void SetFeedback(RectTransform newTarget, float scale, float lerpSpeed)
    {
        targetRect = newTarget;
        hoverScale = scale;
        hoverLerpSpeed = lerpSpeed;
        ApplyScalePivot();
        CacheBaseTransform();
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
        ResetTargetTransform();
    }

    private void CacheBaseTransform()
    {
        var rect = target;
        if (rect == null)
            return;

        baseScale = rect.localScale;
    }

    private void ApplyScalePivot()
    {
        if (!scaleFromCenter)
            return;

        var rect = target;
        if (rect == null)
            return;

        var centerPivot = new Vector2(0.5f, 0.5f);
        if (rect.pivot == centerPivot)
            return;

        var centerBefore = rect.TransformPoint(rect.rect.center);
        rect.pivot = centerPivot;
        var centerAfter = rect.TransformPoint(rect.rect.center);
        rect.position += centerBefore - centerAfter;
    }

    private void ResetTargetTransform()
    {
        var rect = target;
        if (rect == null)
            return;

        rect.localScale = baseScale;
    }
}

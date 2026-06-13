using UnityEngine;

public class TitleBackgroundMotion : MonoBehaviour
{
    [SerializeField] private bool enableMotion = true;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float cycleDuration = 8f;
    [SerializeField] private float scaleAmount = 0.015f;
    [SerializeField] private Vector2 positionOffset = new Vector2(4f, 3f);
    [SerializeField] private float phaseOffset = 0f;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private float elapsed;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        CacheBaseTransform();
    }

    private void OnEnable()
    {
        CacheBaseTransform();
    }

    private void Update()
    {
        if (!enableMotion || rectTransform == null)
            return;

        elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        var duration = Mathf.Max(0.01f, cycleDuration);
        var phase = ((elapsed / duration) * Mathf.PI * 2f) + phaseOffset;
        var scale = 1f + ((Mathf.Sin(phase) + 1f) * 0.5f * scaleAmount);
        var offset = new Vector2(
            Mathf.Sin(phase * 0.73f) * positionOffset.x,
            Mathf.Cos(phase * 0.61f) * positionOffset.y
        );

        rectTransform.localScale = baseScale * scale;
        rectTransform.anchoredPosition = basePosition + offset;
    }

    private void OnDisable()
    {
        ResetTransform();
    }

    private void OnValidate()
    {
        cycleDuration = Mathf.Max(0.01f, cycleDuration);
        scaleAmount = Mathf.Max(0f, scaleAmount);
    }

    private void CacheBaseTransform()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        if (rectTransform == null)
            return;

        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
    }

    private void ResetTransform()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = baseScale;
    }
}

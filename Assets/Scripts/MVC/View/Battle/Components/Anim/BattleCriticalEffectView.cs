using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleCriticalEffectView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private RectTransform rect;

    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }
    }

    public void Play(Sprite sprite, BattleCriticalEffectConfig config, Vector2 centerPosition, bool isMe)
    {
        if ((sprite == null) || (config == null) || (image == null) || (rect == null))
        {
            Destroy(gameObject);
            return;
        }

        float dir = isMe ? 1f : -1f;
        Vector2 positionOffset = new Vector2(config.AnchoredPosition.x * dir, config.AnchoredPosition.y);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) * config.Scale;
        rect.anchoredPosition = centerPosition + positionOffset;
        rect.localScale = new Vector3(dir, 1, 1);

        image.sprite = sprite;
        image.raycastTarget = false;
        image.preserveAspect = false;

        StartCoroutine(PlayAnim(config, dir));
    }

    private IEnumerator PlayAnim(BattleCriticalEffectConfig config, float dir)
    {
        if (config.Duration <= 0)
        {
            Destroy(gameObject);
            yield break;
        }

        float time = 0;
        Vector2 centerPos = rect.anchoredPosition;
        Vector2 offset = new Vector2(config.MoveOffset.x * dir, config.MoveOffset.y);
        Vector2 startPos = centerPos - offset;
        Vector2 endPos = centerPos + offset;
        Color color = image.color;

        while ((time < config.Duration) && (rect != null) && (image != null))
        {
            float t = time / config.Duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseOut(t));
            rect.localScale = new Vector3(dir, 1, 1);
            color.a = GetAlpha(t);
            image.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3);
    }

    private float GetAlpha(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.18f)
            return Mathf.Lerp(0, 1, t / 0.18f);

        return Mathf.Lerp(1, 0, Mathf.InverseLerp(0.62f, 1f, t));
    }
}

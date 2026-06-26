using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDamageAnimView : Module
{
    [SerializeField] private Vector2 damageAnchoredPos;
    [SerializeField] private RectTransform damageAnchoredRect;
    [SerializeField] private GameObject damageBackgroundPrefab;
    [SerializeField] private GameObject healAnchoredObject;
    [SerializeField] private GameObject healNumberPrefab;
    [SerializeField] private FightCamaraController camara;
    [SerializeField] private float comboDamagePreviewInterval = 0.18f;
    [SerializeField] private float comboDamagePreviewMinOffset = 50f;
    [SerializeField] private float comboDamagePreviewMaxOffset = 70f;
    [SerializeField] private float comboDamagePreviewHoldTime = 0.35f;

    private SettingsData settingsData => Player.instance.gameData.settingsData;
    private readonly Dictionary<string, Sprite> criticalEffectSpriteDict = new Dictionary<string, Sprite>();
    private RectTransform criticalEffectLayer;
    public bool isDone = true;

    public void SetHealObject(UnitHudSystem.HealInfo info)
    {
        if ((info.Heal == 0) && (!info.IsForceShowHeal))
            return;

        GameObject obj = Instantiate(healNumberPrefab, healAnchoredObject.transform);
        Text num = obj.GetComponent<Text>();
        num.text = info.HealText;
        num.color = info.TypeColor;
        isDone = false;
        StartCoroutine(SetHealAnim(num.rectTransform, string.IsNullOrEmpty(info.Type) || info.Type == "item"));
    }

    /// <summary>
    /// Set heal animation.
    /// </summary>
    /// <param name="healRect">Object</param>
    /// <param name="dir">True for down. False for up.</param>
    /// <returns>IEnumerator for coroutine use.</returns>
    private IEnumerator SetHealAnim(RectTransform healRect, bool dir = true)
    {
        // float time = 0;
        float speed = (dir ? -1.5f : 3f) * ((settingsData.battleAnimSpeed - 1) / 2f + 1);

        if (dir)
        {
            while (healRect.anchoredPosition.y > -50)
            {
                healRect.anchoredPosition = new Vector2(healRect.anchoredPosition.x, healRect.anchoredPosition.y + speed);
                yield return new WaitForSeconds(0.02f);
            }   
        } 
        else
        {   
            healRect.anchoredPosition = new Vector2(healRect.anchoredPosition.x, healRect.anchoredPosition.y - 50);

            yield return new WaitForSeconds(0.2f);

            while (healRect.anchoredPosition.y < 25)
            {
                healRect.anchoredPosition = new Vector2(healRect.anchoredPosition.x, healRect.anchoredPosition.y + speed);
                yield return new WaitForSeconds(0.02f);
            }    
        }

        isDone = true;

        yield return new WaitForSeconds(0.8f);
        Destroy(healRect.gameObject);
    }

    public void SetDamageObject(UnitHudSystem.DamageInfo info, Vector3? criticalEffectWorldPosition = null)
    {
        bool hasComboDamageInfo = (info.ComboDamageInfoList != null) && (info.ComboDamageInfoList.Count > 1);
        ComboDamageDisplayMode comboMode =
            VolumeSettingModel.NormalizeComboDamageDisplayMode(settingsData.comboDamageDisplayMode);
        if (hasComboDamageInfo && comboMode != ComboDamageDisplayMode.TotalOnly)
        {
            StartCoroutine(SetComboDamageObject(info, criticalEffectWorldPosition, comboMode));
            return;
        }

        SetDamageObject(info, info.Damage, info.IsCritical, true, criticalEffectWorldPosition);
    }

    private IEnumerator SetComboDamageObject(UnitHudSystem.DamageInfo info, Vector3? criticalEffectWorldPosition,
        ComboDamageDisplayMode comboMode)
    {
        var comboDamageObjects = new List<GameObject>();
        foreach (var comboDamageInfo in info.ComboDamageInfoList)
        {
            var obj = SetDamageObject(info, comboDamageInfo.Damage, comboDamageInfo.IsCritical, false,
                criticalEffectWorldPosition, false);
            comboDamageObjects.Add(obj);
            yield return new WaitForSeconds(comboDamagePreviewInterval / Mathf.Max(settingsData.battleAnimSpeed, 1f));
        }

        if (comboMode == ComboDamageDisplayMode.AllComboAndTotal)
        {
            var finalDamageObject = SetDamageObject(info, info.Damage, info.IsCritical, true, criticalEffectWorldPosition);
            yield return new WaitWhile(() => finalDamageObject != null);
            DestroyComboDamageObjects(comboDamageObjects);
        }
        else
        {
            SetFinalDamageEffects(info, info.Damage, info.IsCritical, criticalEffectWorldPosition, false);
            yield return new WaitForSeconds(comboDamagePreviewHoldTime / Mathf.Max(settingsData.battleAnimSpeed, 1f));
            DestroyComboDamageObjects(comboDamageObjects);
        }
    }

    private GameObject SetDamageObject(UnitHudSystem.DamageInfo info, int damage, bool isCritical, bool isFinalDamage,
        Vector3? criticalEffectWorldPosition, bool destroyOnAnimEnd = true)
    {
        bool isHit = info.IsHit;
        bool isMe = info.IsMe;
        float elementRelation = info.ElementRelation;
        bool isDamage = (isHit && (damage != 0));
        string who = isMe ? "me" : "op";
        string absorb = isDamage ? string.Empty : "Absorb";

        GameObject holder = CreateDamageObjectHolder(isFinalDamage);
        GameObject obj = Instantiate(damageBackgroundPrefab, holder.transform);
        BattleDamageBackgroundView script = obj.GetComponent<BattleDamageBackgroundView>();
        Image img = script.Background;
        IAnimator anim = script.Anim;
        script.Critical.SetActive(isDamage && isCritical);
        if (isFinalDamage)
        {
            SetFinalDamageEffects(info, damage, isCritical, criticalEffectWorldPosition, true);
        }

        script.Rect.anchoredPosition = damageAnchoredPos;
        holder.transform.SetAsLastSibling();

        if (isDamage || (!isFinalDamage && isHit))
        {
            script.InstantiateDamageNum(damage, isCritical);
        }

        img.SetDamageBackgroundSprite(elementRelation, isHit, (damage == 0));
        obj.SetActive(true);

        SetDamageAnim(anim, who + absorb, destroyOnAnimEnd, holder);
        return holder;
    }

    private GameObject CreateDamageObjectHolder(bool isFinalDamage)
    {
        var holder = new GameObject("Damage Object Holder", typeof(RectTransform));
        var rect = holder.GetComponent<RectTransform>();
        rect.SetParent(damageAnchoredRect, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = GetComboDamagePreviewOffset(isFinalDamage);
        return holder;
    }

    private Vector2 GetComboDamagePreviewOffset(bool isFinalDamage)
    {
        if (isFinalDamage)
            return Vector2.zero;

        float offsetMin = Mathf.Min(comboDamagePreviewMinOffset, comboDamagePreviewMaxOffset);
        float offsetMax = Mathf.Max(comboDamagePreviewMinOffset, comboDamagePreviewMaxOffset);
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = UnityEngine.Random.Range(offsetMin, offsetMax);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    private void DestroyComboDamageObjects(List<GameObject> comboDamageObjects)
    {
        foreach (var obj in comboDamageObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    private void SetFinalDamageEffects(UnitHudSystem.DamageInfo info, int damage, bool isCritical,
        Vector3? criticalEffectWorldPosition, bool scaleDamageAnchor)
    {
        bool isDamage = info.IsHit && (damage != 0);
        if (isDamage && isCritical)
        {
            SetCriticalEffectObject(info, criticalEffectWorldPosition);
        }

        if (scaleDamageAnchor)
        {
            damageAnchoredRect.transform.localScale =
                (isDamage && (isCritical || damage > 500)) ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one;
        }

        if (!info.DamageType || !isDamage)
            return;

        if (isCritical || (settingsData.shakeWhenBigDamage && (damage > 400)))
        {
            camara.ScreenShake();
        }

        if (settingsData.flashWhenBigDamage && (damage > 500))
        {
            camara.ScreenFlash();
        }
    }

    private void SetCriticalEffectObject(UnitHudSystem.DamageInfo info, Vector3? criticalEffectWorldPosition)
    {
        var config = BattleCriticalEffectConfig.Get(info.AttackPetBaseId);
        if (config == null)
            return;

        Sprite sprite = GetCriticalEffectSprite(config.ResourcePath);
        if (sprite == null)
            return;

        RectTransform effectParent = GetCriticalEffectParent();
        GameObject obj = new GameObject("Critical Effect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        Image img = obj.GetComponent<Image>();
        obj.transform.SetParent(effectParent, false);
        obj.transform.SetAsFirstSibling();

        SetCriticalEffectRect(rect, sprite, config, criticalEffectWorldPosition);
        img.sprite = sprite;
        img.raycastTarget = false;
        img.preserveAspect = false;
        StartCoroutine(SetCriticalEffectAnim(rect, img, config, info.IsMe));
    }

    private RectTransform GetCriticalEffectParent()
    {
        if (criticalEffectLayer == null)
        {
            criticalEffectLayer = GameObject.Find("UI Layer")?.GetComponent<RectTransform>();
        }

        return criticalEffectLayer != null ? criticalEffectLayer : damageAnchoredRect;
    }

    private void SetCriticalEffectRect(RectTransform rect, Sprite sprite, BattleCriticalEffectConfig config,
        Vector3? criticalEffectWorldPosition)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height) * config.Scale;
        rect.anchoredPosition = GetCriticalEffectAnchoredPosition(rect.parent as RectTransform,
            criticalEffectWorldPosition) + config.AnchoredPosition;
    }

    private Sprite GetCriticalEffectSprite(string path)
    {
        if (criticalEffectSpriteDict.TryGetValue(path, out var sprite))
            return sprite;

        Texture2D texture = Resources.Load<Texture2D>("Sprites/" + path);
        if (texture == null)
            return null;

        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        criticalEffectSpriteDict.Set(path, sprite);
        return sprite;
    }

    private IEnumerator SetCriticalEffectAnim(RectTransform rect, Image img, BattleCriticalEffectConfig config, bool isMe)
    {
        float time = 0;
        float dir = isMe ? 1f : -1f;
        Vector2 centerPos = rect.anchoredPosition;
        Vector2 offset = new Vector2(config.MoveOffset.x * dir, config.MoveOffset.y);
        Vector2 startPos = centerPos - offset;
        Vector2 endPos = centerPos + offset;
        Color color = img.color;

        while ((time < config.Duration) && (rect != null) && (img != null))
        {
            float t = time / config.Duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseOut(t));
            rect.localScale = new Vector3(dir, 1, 1);
            color.a = GetCriticalEffectAlpha(t);
            img.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        if (rect != null)
        {
            Destroy(rect.gameObject);
        }
    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3);
    }

    private Vector2 GetCriticalEffectAnchoredPosition(RectTransform parent, Vector3? worldPosition)
    {
        if ((parent == null) || (Camera.main == null) || !worldPosition.HasValue)
            return damageAnchoredPos;

        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition.Value);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, uiCamera, out var localPos)
            ? localPos
            : damageAnchoredPos;
    }

    private float GetCriticalEffectAlpha(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.18f)
            return Mathf.Lerp(0, 1, t / 0.18f);

        return Mathf.Lerp(1, 0, Mathf.InverseLerp(0.62f, 1f, t));
    }

    private void SetDamageAnim(IAnimator iAnim, string trigger, bool destroyOnAnimEnd = true, GameObject destroyObject = null)
    {
        Action onAnimEnd = destroyOnAnimEnd ? () => { OnDamageAnimEnd(iAnim, destroyObject); } : () => { };
        iAnim.onAnimEndEvent.SetListener(onAnimEnd.Invoke);
        iAnim.anim.ResetAllTriggers();
        iAnim.anim.SetTrigger(trigger);
    }

    private void OnDamageAnimEnd(IAnimator anim, GameObject destroyObject = null)
    {
        damageAnchoredRect.transform.localScale = Vector3.one;
        Destroy(destroyObject != null ? destroyObject : anim.gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDamageAnimView : Module
{
    private const string CriticalEffectPrefabPath = "Fight/Critical Effect";

    [SerializeField] private Vector2 damageAnchoredPos;
    [SerializeField] private RectTransform damageAnchoredRect;
    [SerializeField] private GameObject damageBackgroundPrefab;
    [SerializeField] private BattleCriticalEffectView criticalEffectPrefab;
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
        foreach (var comboDamageInfo in info.ComboDamageInfoList)
        {
            SetDamageObject(info, comboDamageInfo.Damage, comboDamageInfo.IsCritical, false,
                criticalEffectWorldPosition);
            yield return new WaitForSeconds(comboDamagePreviewInterval / Mathf.Max(settingsData.battleAnimSpeed, 1f));
        }

        if (comboMode == ComboDamageDisplayMode.AllComboAndTotal)
        {
            var finalDamageObject = SetDamageObject(info, info.Damage, info.IsCritical, true, criticalEffectWorldPosition);
            yield return new WaitWhile(() => finalDamageObject != null);
        }
        else
        {
            SetFinalDamageEffects(info, info.Damage, info.IsCritical, criticalEffectWorldPosition, false);
            yield return new WaitForSeconds(comboDamagePreviewHoldTime / Mathf.Max(settingsData.battleAnimSpeed, 1f));
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
        var config = BattleCriticalEffectConfig.GetCriticalPoster(info.AttackPetAnimId, info.AttackPetBaseId);
        if (config == null)
            return;

        Sprite sprite = GetCriticalEffectSprite(config);
        if (sprite == null)
            return;

        RectTransform effectParent = GetCriticalEffectParent();
        BattleCriticalEffectView effectView = CreateCriticalEffectView(effectParent);
        if (effectView == null)
            return;

        Vector2 centerPosition = GetCriticalEffectAnchoredPosition(effectParent, criticalEffectWorldPosition);
        effectView.Play(sprite, config, centerPosition, info.IsMe);
    }

    private RectTransform GetCriticalEffectParent()
    {
        if (criticalEffectLayer == null)
        {
            criticalEffectLayer = GameObject.Find("UI Layer")?.GetComponent<RectTransform>();
        }

        return criticalEffectLayer != null ? criticalEffectLayer : damageAnchoredRect;
    }

    private BattleCriticalEffectView CreateCriticalEffectView(RectTransform parent)
    {
        BattleCriticalEffectView prefab = criticalEffectPrefab;
        if (prefab == null)
        {
            prefab = ResourceManager.instance.GetPrefab(CriticalEffectPrefabPath)?.GetComponent<BattleCriticalEffectView>();
        }

        if (prefab == null)
            return null;

        BattleCriticalEffectView effectView = Instantiate(prefab, parent);
        effectView.transform.SetAsFirstSibling();
        return effectView;
    }

    private Sprite GetCriticalEffectSprite(BattleCriticalEffectConfig config)
    {
        string cacheKey = $"{config.IsMod}:{config.ResolvedAssetKey}";
        if (criticalEffectSpriteDict.TryGetValue(cacheKey, out var sprite))
            return sprite;

        sprite = ResourceManager.instance.GetLocalAddressables<Sprite>(config.ResolvedAssetKey, config.IsMod);
        if ((sprite == null) && !config.IsMod)
        {
            sprite = ResourceManager.instance.GetSprite(config.ResolvedAssetKey);
        }

        if (sprite == null)
            return null;

        criticalEffectSpriteDict.Set(cacheKey, sprite);
        return sprite;
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

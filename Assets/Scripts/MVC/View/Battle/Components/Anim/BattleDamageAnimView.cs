using System;
using System.Collections;
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
    [SerializeField] private float comboDamagePreviewLifetime = 0.28f;

    private SettingsData settingsData => Player.instance.gameData.settingsData;
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

    public void SetDamageObject(UnitHudSystem.DamageInfo info)
    {
        if ((info.ComboDamageInfoList != null) && (info.ComboDamageInfoList.Count > 1))
        {
            StartCoroutine(SetComboDamageObject(info));
            return;
        }

        SetDamageObject(info, info.Damage, info.IsCritical, true);
    }

    private IEnumerator SetComboDamageObject(UnitHudSystem.DamageInfo info)
    {
        foreach (var comboDamageInfo in info.ComboDamageInfoList)
        {
            var obj = SetDamageObject(info, comboDamageInfo.Damage, comboDamageInfo.IsCritical, false);
            Destroy(obj, comboDamagePreviewLifetime / Mathf.Max(settingsData.battleAnimSpeed, 1f));
            yield return new WaitForSeconds(comboDamagePreviewInterval / Mathf.Max(settingsData.battleAnimSpeed, 1f));
        }

        SetDamageObject(info, info.Damage, info.IsCritical, true);
    }

    private GameObject SetDamageObject(UnitHudSystem.DamageInfo info, int damage, bool isCritical, bool isFinalDamage)
    {
        bool type = info.DamageType;
        bool isHit = info.IsHit;
        bool isMe = info.IsMe;
        float elementRelation = info.ElementRelation;
        bool isDamage = (isHit && (damage != 0));
        string who = isMe ? "me" : "op";
        string absorb = isDamage ? string.Empty : "Absorb";

        GameObject obj = Instantiate(damageBackgroundPrefab, damageAnchoredRect);
        BattleDamageBackgroundView script = obj.GetComponent<BattleDamageBackgroundView>();
        Image img = script.Background;
        IAnimator anim = script.Anim;
        script.Critical.SetActive(isDamage && isCritical);
        if (isFinalDamage)
        {
            damageAnchoredRect.transform.localScale =
                (isDamage && (isCritical || damage > 500)) ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one;
        }

        if (isFinalDamage && type && isDamage)
        {
            if (isCritical || (settingsData.shakeWhenBigDamage && (damage > 400)))
            {
                camara.ScreenShake();
            }

            if (settingsData.flashWhenBigDamage && (damage > 500))
            {
                camara.ScreenFlash();
            }
        }

        script.Rect.anchoredPosition = damageAnchoredPos;
        script.Rect.SetAsLastSibling();

        if (isDamage)
        {
            script.InstantiateDamageNum(damage, isCritical);
        }

        img.SetDamageBackgroundSprite(elementRelation, isHit, (damage == 0));
        obj.SetActive(true);

        SetDamageAnim(anim, who + absorb);
        return obj;
    }

    private void SetDamageAnim(IAnimator iAnim, string trigger)
    {
        Action onAnimEnd = () => { OnDamageAnimEnd(iAnim); };
        iAnim.onAnimEndEvent.SetListener(onAnimEnd.Invoke);
        iAnim.anim.ResetAllTriggers();
        iAnim.anim.SetTrigger(trigger);
    }

    private void OnDamageAnimEnd(IAnimator anim)
    {
        damageAnchoredRect.transform.localScale = Vector3.one;
        Destroy(anim.gameObject);
    }
}

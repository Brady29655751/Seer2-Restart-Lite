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

    private SettingsData settingsData => Player.instance.gameData.settingsData;

    public void SetHealObject(UnitHudSystem.HealInfo info)
    {
        if ((info.Heal == 0) && (!info.IsForceShowHeal))
            return;

        GameObject obj = Instantiate(healNumberPrefab, healAnchoredObject.transform);
        Text num = obj.GetComponent<Text>();
        num.text = info.HealText;
        num.color = info.TypeColor;
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
        float speed = dir ? -1.5f : 3f;

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

        yield return new WaitForSeconds(0.8f);
        Destroy(healRect.gameObject);
    }

    public void SetDamageObject(UnitHudSystem.DamageInfo info)
    {
        //拆包开始
        bool type = info.DamageType;
        bool isHit = info.IsHit;
        bool isCritical = info.IsCritical;
        bool isMe = info.IsMe;
        int damage = info.Damage;
        float elementRelation = info.ElementRelation;
        //拆包结束
        bool isDamage = (isHit && (damage != 0));
        string who = isMe ? "me" : "op";
        string absorb = isDamage ? string.Empty : "Absorb";

        GameObject obj = Instantiate(damageBackgroundPrefab, damageAnchoredRect);
        BattleDamageBackgroundView script = obj.GetComponent<BattleDamageBackgroundView>();
        Image img = script.Background; //设置伤害背景颜色
        IAnimator anim = script.Anim;
        script.Critical.SetActive(isDamage && isCritical);
        damageAnchoredRect.transform.localScale =
            (isDamage && (isCritical || damage > 500)) ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one; //暴击或伤害大的时候放大
        if (type && isDamage)   // 攻擊傷害才適用屏幕效果
        {
            if (isCritical || (settingsData.shakeWhenBigDamage && (damage > 400)))
            {
                camara.ScreenShake(); //暴击或伤害较大时屏幕大幅震动
            }

            if (settingsData.flashWhenBigDamage && (damage > 500)) //伤害过大时屏幕闪烁
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
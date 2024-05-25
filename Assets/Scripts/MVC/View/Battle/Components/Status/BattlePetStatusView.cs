using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetStatusView : BattleBaseView
{
    private Color hpBarNormalColor => new Color32(119, 226, 12, 255);
    [SerializeField] private float hpBarLength = 260, angerBarLength = 246;
    [SerializeField] private Image hpBarImage, angerBarImage;
    [SerializeField] private RectTransform hpBar, angerBar;
    [SerializeField] private Text currentHpText, maxHpText;
    [SerializeField] private Text currentAngerText, maxAngerText;

    public bool isDone => isHpDone && isAngerDone;
    protected bool isHpDone = true;
    protected bool isAngerDone = true;

    public static float animSpeed => Player.instance.gameData.settingsData.battleAnimSpeed;

    public void SetHp(int hp, int maxHp) {
        SetHpText(hp, maxHp);
        SetHpBar(hp, maxHp);
    }

    public void SetHp(int lastHp, int lastMaxHp, int hp, int maxHp) {
        StartCoroutine(ModifyHp(lastHp, lastMaxHp, hp, maxHp));
    }

    private IEnumerator ModifyHp(int lastHp, int lastMaxHp, int hp, int maxHp) {
        isHpDone = false;
        SetHpText(hp, maxHp);

        float currentHp = lastHp;
        float percent = Mathf.Abs((hp - lastHp) * 100f / maxHp);
        float speed = 0.005f + 0.01f * Mathf.Max(0, (percent - 30) / 70) * animSpeed;
        float diff = (hp == lastHp) ? 0 : (hp - currentHp) / (hp - lastHp);
        while (diff > 0.02f) {
            currentHp += ((speed * maxHp) * Mathf.Sign(hp - lastHp));
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            diff = (hp == lastHp) ? 0 : (hp - currentHp) / (hp - lastHp);
            SetHpBar(currentHp, maxHp);
            yield return new WaitForSeconds(0.02f);
        }
        SetHpBar(hp, maxHp);
        isHpDone = true;
    }

    private void SetHpText(int hp, int maxHp) {
        currentHpText.text = hp.ToString();
        maxHpText.text = maxHp.ToString();
    }

    private void SetHpBar(float hp, float maxHp) {
        float percent = hp / maxHp;
        hpBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, percent * hpBarLength);

        Color highHpColor = Color.Lerp(new Color(1, 1, 0, 1), hpBarNormalColor, (percent - 0.5f) * 2);
        Color lowHpColor = Color.Lerp(Color.red, new Color(1, 1, 0, 1), percent * 2); 

        hpBarImage.color = (percent >= 0.5f) ? highHpColor : lowHpColor;
    }

    public void SetAnger(int anger, int maxAnger) {
        SetAngerText(anger, maxAnger);
        SetAngerBar(anger, maxAnger);
    }

    public void SetAnger(int lastAnger, int lastMaxAnger, int anger, int maxAnger) {
        StartCoroutine(ModifyAnger(lastAnger, lastMaxAnger, anger, maxAnger));
    }

    private IEnumerator ModifyAnger(int lastAnger, int lastMaxAnger, int anger, int maxAnger) {
        isAngerDone = false;
        SetAngerText(anger, maxAnger);

        float currentAnger = lastAnger;
        float percent = Mathf.Abs((anger - lastAnger) * 100f / maxAnger);
        float speed = 0.01f + 0.005f * Mathf.Max(0, (percent - 30) / 70) * animSpeed;
        float diff = (anger == lastAnger) ? 0 : (anger - currentAnger) / (anger - lastAnger);
        while (diff > 0.02f) {
            currentAnger += ((speed * maxAnger) * Mathf.Sign(anger - lastAnger));
            currentAnger = Mathf.Clamp(currentAnger, 0, maxAnger);
            diff = (anger == lastAnger) ? 0 : (anger - currentAnger) / (anger - lastAnger);
            SetAngerBar(currentAnger, maxAnger);
            yield return new WaitForSeconds(0.02f);
        }
        SetAngerBar(anger, maxAnger);
        isAngerDone = true;
    }

    private void SetAngerText(int anger, int maxAnger) {
        currentAngerText.text = anger.ToString();
        maxAngerText.text = maxAnger.ToString();
    }

    private void SetAngerBar(float anger, float maxAnger) {
        float percent = anger / maxAnger;
        angerBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, percent * angerBarLength);
    }
}

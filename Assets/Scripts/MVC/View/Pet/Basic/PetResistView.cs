using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetResistView : UIModule
{
    [SerializeField] private GameObject disabledBackground, lockedBackground, openedBackground, confirmButton, resetButton, maxButton; 
    [SerializeField] private List<PetStatusBlockView> damageValueBlockViews = new List<PetStatusBlockView>();
    [SerializeField] private List<BattlePetBuffBlockView> buffTypeBlockViews = new List<BattlePetBuffBlockView>();
    [SerializeField] private List<PetStatusBlockView> buffValueBlockViews = new List<PetStatusBlockView>();

    public void SetPetResist(PetResist resist)
    {
        if (resist == null)
        {
            SetBackground(-1);
            return;
        }

        SetBackground(resist.lockState);
        SetDamageResist(resist);
        SetBuffResistType(resist);
        SetBuffResistValue(resist);
    }

    private void SetBackground(int lockState)
    {
        disabledBackground.gameObject.SetActive(lockState < 0);
        lockedBackground.gameObject.SetActive(lockState == 0);
        openedBackground.gameObject.SetActive(lockState > 0);
    }

    private void SetDamageResist(PetResist resist)
    {
        damageValueBlockViews[0].SetEV(resist.criResist);
        damageValueBlockViews[1].SetEV(resist.fixResist);
        damageValueBlockViews[2].SetEV(resist.perResist);
    }

    private void SetBuffResistType(PetResist resist)
    {
        buffTypeBlockViews[0].SetBuff(new Buff(resist.abnormalBuffId));
        buffTypeBlockViews[1].SetBuff(new Buff(resist.unhealthyBuffId));   
    }

    private void SetBuffResistValue(PetResist resist)
    {
        buffValueBlockViews[0].SetEV(resist.abnormalResist.value);
        buffValueBlockViews[1].SetEV(resist.unhealthyResist.value);
    }

    public void ShowBuffInfo(Buff buff)
    {
        var showAtRight = buff.IsType(BuffType.Abnormal);
        infoPrompt.SetBuff(buff, showAtRight);

        var fixPos = showAtRight ? new Vector2(12, infoPrompt.zeroFixPos.y) : new Vector2(-infoPrompt.size.x - 2, infoPrompt.zeroFixPos.y);
        infoPrompt.SetPositionOffset(fixPos);
    }

    public void SetEVButtonsActive(bool active)
    {
        damageValueBlockViews.ForEach(blockView => 
        {
            blockView.SetEVButtonsActive(active, true);
            blockView.SetEVButtonsActive(active, false);
        });

        buffValueBlockViews.ForEach(blockView => 
        {
            blockView.SetEVButtonsActive(active, true);
            blockView.SetEVButtonsActive(active, false);
        });

        confirmButton.gameObject.SetActive(active);
        resetButton.gameObject.SetActive(!active);
        maxButton.gameObject.SetActive(!active);
    }

}

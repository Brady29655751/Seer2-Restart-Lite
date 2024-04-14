using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobRewardController : Module
{
    [SerializeField] private NoobRewardModel noobRewardModel;
    [SerializeField] private NoobRewardView noobRewardView;

    public override void Init() {
        RefreshView();
        noobRewardView.SetRewardIcons(noobRewardModel.rewardIcons);
    }

    public void RefreshView() {
        noobRewardView.SetSignDays(noobRewardModel.signedDays);
        noobRewardView.SetTodaySigned(noobRewardModel.isTodaySigned);
    }

    public void Sign() {
        if ((noobRewardModel.isTodaySigned) || (noobRewardModel.signedDays >= 7))
            return;
        
        noobRewardModel.Sign();
        RefreshView();
    }
}

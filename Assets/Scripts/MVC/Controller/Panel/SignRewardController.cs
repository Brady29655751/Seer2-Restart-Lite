using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignRewardController : Module
{
    [SerializeField] private SignRewardModel signRewardModel;
    [SerializeField] private SignRewardView signRewardView;

    public override void Init() {
        SetActivity(signRewardModel.activityId);
    }

    public void SetActivity(string activityId) {
        signRewardModel.SetActivity(activityId);
        RefreshView();
    }

    public void RefreshView() {
        signRewardView.SetTitle(signRewardModel.activity.info.name);
        signRewardView.SetSignDays(signRewardModel.signedDays);
        signRewardView.SetTodaySigned(signRewardModel.isTodaySigned);
        signRewardView.SetRewardIcons(signRewardModel.rewardIcons);
    }

    public void Sign() {
        if ((signRewardModel.isTodaySigned) || (signRewardModel.signedDays >= 7))
            return;
        
        signRewardModel.Sign();
        RefreshView();
    }
}

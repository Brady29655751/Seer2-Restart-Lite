using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStatusView : BattleBaseView
{
    [SerializeField] private BattlePetStatusView statusView;
    [SerializeField] private BattlePetBuffView buffView;
    [SerializeField] private BattlePetInfoView infoView;

    public bool isDone => statusView.isDone;

    public void SetPet(BattlePet lastPet, BattlePet currentPet) {
        if (currentPet == null)
        {
            statusView.SetHp(0, 1);
            statusView.SetAnger(0, 1);
            infoView.SetPet(currentPet);
            return;
        }

        if (lastPet == null) {
            statusView.SetHp(currentPet.hp, currentPet.maxHp);
            statusView.SetAnger(currentPet.anger, currentPet.maxAnger);
        } else {
            statusView.SetHp(lastPet.hp, lastPet.maxHp, currentPet.hp, currentPet.maxHp);
            statusView.SetAnger(lastPet.anger, lastPet.maxAnger, currentPet.anger, currentPet.maxAnger);
        }
        buffView.SetBuff(currentPet.buffs);
        infoView.SetPet(currentPet);
    }


}

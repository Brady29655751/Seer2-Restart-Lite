using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BattleUnitView : BattleBaseView
{
    [SerializeField] protected bool isMe = true;
    [SerializeField] protected BattleStatusView statusView;
    [SerializeField] protected BattlePetNumView petNumView;
    [SerializeField] protected BattleAnimView animView;

    public bool isDone => statusView.isDone && animView.isDone;

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;
        
        var lastUnit = isMe ? (lastState?.myUnit) : (lastState?.opUnit);
        var currentUnit = isMe ? (currentState?.myUnit) : (currentState?.opUnit);

        statusView.SetPet(lastUnit?.pet, currentUnit?.pet);
        petNumView.SetPetBag(currentUnit.petSystem.petBag);
        animView.SetUnit(lastUnit, currentUnit);
    }

    [PunRPC]
    private void RPCSetSkill(string[] skillData) {
        var skill = Skill.ParseRPCData(skillData);
        battle.SetSkill(skill, false);
    }

}

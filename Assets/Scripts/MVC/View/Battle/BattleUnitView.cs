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
    [SerializeField] protected BattlePetBuffView buffView;

    public bool isDone => statusView.isDone && animView.isDone;

    public void SetState(BattleState lastState, BattleState currentState, int cursorOffset = 0) {
        if (currentState == null)
            return;
        
        var parallelCount = battle.settings.parallelCount;
        var lastUnit = isMe ? (lastState?.myUnit) : (lastState?.opUnit);
        var currentUnit = isMe ? (currentState?.myUnit) : (currentState?.opUnit);
        var lastPetSystem = lastUnit?.petSystem;
        var currentPetSystem = currentUnit?.petSystem;
        var lastPet = lastPetSystem?.GetParallelPetBag(parallelCount)?.Get(cursorOffset);
        var currentPet = currentPetSystem?.GetParallelPetBag(parallelCount)?.Get(cursorOffset);

        statusView.SetPet(lastPet, currentPet);
        petNumView.SetPetBag(currentUnit.petSystem.petBag);
        animView.SetUnit(lastUnit, currentUnit, cursorOffset);

        if (currentUnit != null)
            buffView.SetBuff(currentUnit.unitBuffs);
    }

    [PunRPC]
    private void RPCSetSkill(string[] skillData) {
        var skill = Skill.ParseRPCData(skillData);
        battle.SetSkill(skill, false);
    }

}

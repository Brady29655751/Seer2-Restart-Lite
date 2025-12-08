using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundEndPhase : BattlePhase
{
    public RoundEndPhase() 
    {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnRoundEnd;
    }

    public override void DoWork()
    {
        state.whosTurn = 0;
        state.myUnit.pet.anger += 15;
        state.opUnit.pet.anger = int.MaxValue;

        ApplySkillsAndBuffs();
        state.myUnit.cardSystem.OnRoundEnd();
        SetUIState(battle.currentState);
    }

    public override BattlePhase GetNextPhase()
    {
        return null;
    }
}

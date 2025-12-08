using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStartPhase : BattlePhase
{
    public RoundStartPhase() 
    {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnRoundStart;
    }

    public override void DoWork()
    {
        state.whosTurn = 0;
        state.myUnit.pet.anger += 15;
        
        state.myUnit.pet.buffController.ReduceBuffTurn(state.myUnit, state, "option[round]");
        state.myUnit.ReduceBuffTurn("option[round]");

        state.opUnit.pet.buffController.ReduceBuffTurn(state.opUnit, state, "option[round]");
        state.opUnit.ReduceBuffTurn("option[round]");

        state.myUnit.cardSystem.OnRoundStart();

        ApplySkillsAndBuffs();
        SetUIState(state);
    }

    public override BattlePhase GetNextPhase()
    {
        return null;
    }
}

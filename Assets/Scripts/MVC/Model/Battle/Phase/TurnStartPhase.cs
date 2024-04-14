using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStartPhase : BattlePhase
{
    public TurnStartPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnTurnStart;
    }

    public override void DoWork()
    {
        state.OnTurnStart();
        base.DoWork();   
        UI.ProcessQuery();
    }

    public override BattlePhase GetNextPhase()
    {
        return null;
    }
}

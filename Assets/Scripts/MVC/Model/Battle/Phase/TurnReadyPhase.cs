using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnReadyPhase : BattlePhase
{
    public TurnReadyPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnTurnReady;
    }

    public override void DoWork()
    {
        UI.StopTimer();
        base.DoWork();
    }

    public override BattlePhase GetNextPhase()
    {
        return new DecidePriorityPhase();
    }



}

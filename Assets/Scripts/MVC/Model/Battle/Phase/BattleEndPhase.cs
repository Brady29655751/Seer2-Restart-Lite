using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEndPhase : BattlePhase
{
    public BattleEndPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnBattleEnd;
    }

    public override void DoWork()
    {
        ProcessResult();
    }

    public override BattlePhase GetNextPhase()
    {
        return null;
    }

    private void ProcessResult() {
        state.result.ProcessResult(state);
        SetUIState(null);
        UI.ProcessQuery();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEndPhase : BattlePhase
{
    public BattleEndPhase()
    {
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

    private void ProcessResult()
    {
        state.result.ProcessResult(state);
        state.myUnit.hudSystem.OnBattleEnd(state.result.isMyWin);
        state.opUnit.hudSystem.OnBattleEnd(state.result.isOpWin);
        SetUIState(null);
        UI.ProcessQuery();
    }
}
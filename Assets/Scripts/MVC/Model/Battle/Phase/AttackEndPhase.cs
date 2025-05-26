using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEndPhase : BattlePhase
{
    public AttackEndPhase()
    {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnAttackEnd;
    }

    public override void DoWork()
    {
        state.whosTurn = 0;
        base.DoWork();
    }

    public override BattlePhase GetNextPhase()
    {
        return new TurnEndPhase();
    }
}

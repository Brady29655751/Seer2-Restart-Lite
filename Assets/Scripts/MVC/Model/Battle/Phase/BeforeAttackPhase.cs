using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeAttackPhase : BattlePhase
{
    public BeforeAttackPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnBeforeAttack;
    }

    public override void DoWork()
    {
        if (state.isAllUnitDone)
            return;

        if (!IsAttackLegal())
            return;
        
        OnChainStart();
        base.DoWork();
        CheckAccuracy();
    }

    public override BattlePhase GetNextPhase()
    {
        if (state.isAllUnitDone)
            return new TurnEndPhase();

        if (state.atkUnit.skill.isAttack && state.atkUnit.skillSystem.isHit)
            return new DamageCalculatePhase();

        return new AttackPhase();
    }

    private void OnChainStart() {
        state.atkUnit.OnChainStart();
    }

    private void CheckAccuracy() {
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        atkUnit.CalculateAccuracy(defUnit);
    }
}

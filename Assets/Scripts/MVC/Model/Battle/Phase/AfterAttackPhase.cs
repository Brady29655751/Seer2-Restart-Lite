using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterAttackPhase : BattlePhase
{
    public AfterAttackPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnAfterAttack;
    }

    public override void DoWork()
    {
        ApplySkillsAndBuffs();
        RaiseAnger();
        GiveTurnToNextUnit();
        SetUIState(battle.currentState);
    }

    public override BattlePhase GetNextPhase()
    {
        if (state.isAllTurnDone)
            return new TurnEndPhase();

        return new BeforeAttackPhase();
    }

    private void RaiseAnger() {
        var atkUnit = state.atkUnit;
        if (atkUnit.skill.isAttack && atkUnit.skillSystem.isHit) {
            atkUnit.pet.anger += 15;
        }
    }

    private void GiveTurnToNextUnit() {
        state.atkUnit.skill.chain--;
        state.atkUnit.petSystem.chain++;
        state.atkUnit.isDone = state.atkUnit.skill.chain <= 0;
        state.GiveTurnToNextUnit();
    }
}

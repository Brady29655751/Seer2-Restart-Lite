using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculatePhase : BattlePhase
{
    public DamageCalculatePhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnBeforeDamageCalculate;
    }

    public override void DoWork()
    {
        state.atkUnit.PrepareDamageParam(state.defUnit, state.weather);

        SetDamagePhase(EffectTiming.OnBeforeDamageCalculate);
        SetDamagePhase(EffectTiming.OnDamageCalculate);

        state.atkUnit.CalculateDamage(state.defUnit);

        SetDamagePhase(EffectTiming.OnAfterDamageCalculate);
        SetDamagePhase(EffectTiming.OnFinalDamageCalculate);
        
        SetUIState(battle.currentState);
    }

    public override BattlePhase GetNextPhase()
    {
        return new AttackPhase();
    }

    protected void SetDamagePhase(EffectTiming damagePhase) {
        phase = damagePhase;
        ApplySkillsAndBuffs();
    }
}

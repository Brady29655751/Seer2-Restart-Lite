using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPhase : BattlePhase
{
    public AttackPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnAttack;
    }

    public override void DoWork()
    {
        if (state.atkUnit.isDone)
            return;

        if (!IsAttackLegal())
            return;
        
        ApplySkillsAndBuffs();
        ConsumeAnger();
        OnHit();
    }

    public override BattlePhase GetNextPhase()
    {
        if (state.isAllTurnDone)
            return new TurnEndPhase();

        return new AfterAttackPhase();
    }

    private void ConsumeAnger() {
        var lastState = new BattleState(battle.currentState);
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        var hpToAngerBuff = atkUnit.pet.buffController.GetBuff(61);
        var angerDiff = atkUnit.skill.anger - atkUnit.pet.anger;

        if ((hpToAngerBuff != null) && (angerDiff > 0)) {
            atkUnit.pet.hp -= hpToAngerBuff.value * angerDiff;
            atkUnit.pet.anger += angerDiff;
        }

        atkUnit.pet.anger -= atkUnit.skill.anger;
        atkUnit.hudSystem.OnAttack(atkUnit, true);
        defUnit.hudSystem.OnAttack(defUnit, false);
        
        SetUIState(lastState);

        state = new BattleState(state);
        state.atkUnit.hudSystem.OnAttackUndo();
        state.defUnit.hudSystem.OnAttackUndo();
    }

    private void OnHit() {
        var lastState = new BattleState(state);
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;

        if (atkUnit.skillSystem.isHit) {
            defUnit.pet.hp -= atkUnit.skillSystem.totalSkillDamage;
            defUnit.pet.anger += Mathf.FloorToInt((defUnit.pet.maxAnger / 2 - 1) * (1f * atkUnit.skillSystem.totalSkillDamage / defUnit.pet.maxHp) * (defUnit.pet.battleStatus.angrec / 100f));
        }
        atkUnit.hudSystem.OnHit(atkUnit, true);
        defUnit.hudSystem.OnHit(defUnit, false);

        SetUIState(lastState);

        state = new BattleState(state);
        atkUnit = state.atkUnit;
        defUnit = state.defUnit;

        atkUnit.hudSystem.OnHitUndo(true);
        defUnit.hudSystem.OnHitUndo(false);
        
    }

}

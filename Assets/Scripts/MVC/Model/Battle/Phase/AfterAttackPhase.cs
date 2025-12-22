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
        if (state.isAllUnitDone)
            return new AttackEndPhase();

        return new BeforeAttackPhase();
    }

    private void RaiseAnger() {
        if (battle.settings.mode == BattleMode.Card)
            return;

        var atkUnit = state.atkUnit;
        if (atkUnit.skill.isAttack && atkUnit.skillSystem.isHit) {
            atkUnit.pet.anger += 15;
        }
    }

    private void GiveTurnToNextUnit() {
        // 技能行動次數-1，精靈已行動次數+1
        state.atkUnit.skill.chain--;
        state.atkUnit.petSystem.pet.chain++;
        if (state.atkUnit.petSystem.token != null)
            state.atkUnit.petSystem.token.chain++;
        
        state.atkUnit.isDone = true;

        // 如果防禦方有反擊，把主動權交給防禦方，攔截攻擊方的多次行動
        if ((state.defUnit.skillSystem.counterSkill?.type ?? SkillType.空过) != SkillType.空过)
        {
            state.actionOrder.Insert(state.actionCursor + 1, (int)Mathf.Sign(state.defUnit.id) * (state.defUnit.petSystem.cursor + 1));
            state.defUnit.skillSystem.SwapCounterSkill(true);
        }
        // 如果防禦方沒有反擊，且攻擊方還有行動次數，則攻擊方繼續行動 (双方无人阵亡且非空过技能)
        else if ((state.atkUnit.skill.chain > 0) && (!Skill.IsNullOrEmpty(state.atkUnit.skill)) && 
            (!state.atkUnit.pet.isDead) && (!state.defUnit.pet.isDead))
        {
            state.actionCursor--;
        }
        // 如果攻擊方行動結束，且當前行動屬於反擊，則反擊結束並換回正常技能
        else if (state.atkUnit.skillSystem.isCounter)
        {
            state.atkUnit.skillSystem.SwapCounterSkill(false);
        }

        state.GiveTurnToNextUnit();
    }
}

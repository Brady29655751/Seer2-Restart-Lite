using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecidePriorityPhase : BattlePhase
{
    public DecidePriorityPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnDecidePriority;
    }

    public override void DoWork()
    {
        ApplySkillsAndBuffs();
        SetActionOrder(DecidePriority());

        phase = EffectTiming.OnPriorityReady;
        ApplySkillsAndBuffs();
        SetWhosTurn();

        SetUIState(battle.currentState);
    }

    public override BattlePhase GetNextPhase()
    {
        return new BeforeAttackPhase();
    }

    private int DecidePriority() {
        Unit masterUnit = state.masterUnit;
        Unit clientUnit = state.clientUnit;
        Skill masterSkill = masterUnit.skill;
        Skill clientSkill = clientUnit.skill;
        BattlePet masterPet = masterUnit.pet;
        BattlePet clientPet = clientUnit.pet;

        if (masterSkill.isAction && clientSkill.isAction)
            return -1;
        
        if (masterSkill.isAction || clientSkill.isAction)
            return (masterSkill.isAction) ? 1 : -1;
        
        float diffPri = masterSkill.priority - clientSkill.priority;
        if (diffPri != 0)
            return (diffPri > 0) ? 1 : -1;
        
        float diffSpd = masterPet.battleStatus.spd - clientPet.battleStatus.spd;
        if (diffSpd != 0)
            return (diffSpd > 0) ? 1 : -1;
        
        return RandomPriority();
    }

    private int RandomPriority() {
        int rand = Random.Range(0, 2);
        return rand * 2 - 1;
    }

    private void SetActionOrder(int whosTurn) {
        state.actionOrder = new List<int>() { whosTurn, -whosTurn };
    }

    private void SetWhosTurn() {
        state.whosTurn = state.actionOrder.FirstOrDefault();
    }
}

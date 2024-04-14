using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndPhase : BattlePhase
{
    public TurnEndPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnTurnEnd;
    }

    public override void DoWork()
    {
        state.whosTurn = 0;
        
        CheckBattleEnd();

        if (state.result.isBattleEnd)
            return;

        ApplySkillsAndBuffs();
        RaiseAnger();
        OnBuffEffected();
    }

    public override BattlePhase GetNextPhase()
    {
        CheckBattleEnd();

        if (state.result.isBattleEnd)
            return new BattleEndPhase();

        return new TurnStartPhase();
    }

    private void CheckBattleEnd() {
        if (state.myUnit.petSystem.alivePetNum == 0) {
            state.result.state = BattleResultState.Lose;
            return;
        }
        if (state.opUnit.petSystem.alivePetNum == 0) {
            state.result.state = BattleResultState.Win;
            return;
        }
    }

    private void RaiseAnger() {
        state.masterUnit.pet.anger += 15;
        state.clientUnit.pet.anger += 15;
    }

    private void OnBuffEffected() {
        var lastState = new BattleState(battle.currentState);
        var masterUnit = state.masterUnit;
        var clientUnit = state.clientUnit;

        masterUnit.pet.hp += (masterUnit.skillSystem.buffHeal - masterUnit.skillSystem.buffDamage);
        clientUnit.pet.hp += (clientUnit.skillSystem.buffHeal - clientUnit.skillSystem.buffDamage);

        masterUnit.hudSystem.OnTurnEnd(masterUnit, clientUnit);
        clientUnit.hudSystem.OnTurnEnd(clientUnit, masterUnit);

        SetUIState(lastState);

        state = new BattleState(state);
        state.masterUnit.hudSystem.OnTurnEndUndo();
        state.clientUnit.hudSystem.OnTurnEndUndo();
    }

}

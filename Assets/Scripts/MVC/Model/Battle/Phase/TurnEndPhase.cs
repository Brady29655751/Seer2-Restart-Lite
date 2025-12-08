using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var parallelCount = state.settings.parallelCount;
        var masterUnit = state.masterUnit;
        var clientUnit = state.clientUnit;
        var masterPetBag = masterUnit.petSystem.GetParallelPetBag(parallelCount);
        var clientPetBag = clientUnit.petSystem.GetParallelPetBag(parallelCount);

        if (battle.settings.mode != BattleMode.Card)
            masterPetBag.ForEach(x => x.anger += 15);
            
        clientPetBag.ForEach(x => x.anger += 15);
    }

    private void OnBuffEffected() {
        var lastState = new BattleState(battle.currentState);
        var parallelCount = state.settings.parallelCount;
        var masterUnit = state.masterUnit;
        var clientUnit = state.clientUnit;
        var masterPetBag = masterUnit.petSystem.GetParallelPetBag(parallelCount);
        var clientPetBag = clientUnit.petSystem.GetParallelPetBag(parallelCount);

        void ParallelBuffEffected(Unit unit, List<BattlePet> petBag) {
            for (int i = 0; i < petBag.Count; i++)
            {
                var cursor = (parallelCount <= 1) ? 0 : unit.petSystem.petBag.IndexOf(petBag[i]);
                var skillSystem = unit.parallelSkillSystems[cursor];
                var buffDamageShield = petBag[i].buffController.GetBuff(2000);
                var reducedBuffDamage = Mathf.Max(skillSystem.totalBuffDamage - (buffDamageShield?.value ?? 0), 0);
                petBag[i].hp += skillSystem.buffHeal - reducedBuffDamage;

                if (buffDamageShield != null)
                {
                    buffDamageShield.value -= skillSystem.totalBuffDamage;
                    if (buffDamageShield.info.autoRemove && (buffDamageShield.value <= 0))
                    {
                        petBag[i].buffController.RemoveBuff(buffDamageShield, unit, state);
                    }
                }
                    
            }
        }

        ParallelBuffEffected(masterUnit, masterPetBag);
        ParallelBuffEffected(clientUnit, clientPetBag);
        
        masterUnit.hudSystem.OnTurnEnd(masterUnit, clientUnit);
        clientUnit.hudSystem.OnTurnEnd(clientUnit, masterUnit);

        SetUIState(lastState);

        state = new BattleState(state);
        state.masterUnit.hudSystem.OnTurnEndUndo();
        state.clientUnit.hudSystem.OnTurnEndUndo();
    }

}

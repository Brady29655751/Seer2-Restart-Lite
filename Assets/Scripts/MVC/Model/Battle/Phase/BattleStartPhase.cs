using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStartPhase : BattlePhase
{
    public BattleStartPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnBattleStart;
    }

    public override void DoWork()
    {
        SetInitBuffs();
        ApplySkillsAndBuffs();
        SetUIState(null);
    }

    public override BattlePhase GetNextPhase()
    {
        return new TurnStartPhase();
    }

    public void SetInitBuffs() {
        SetInitBuffs(state.masterUnit);
        SetInitBuffs(state.clientUnit);
    }

    private void SetInitBuffs(Unit thisUnit) {
        for (int i = 0; i < thisUnit.petSystem.petBag.Length; i++) {
            thisUnit.petSystem.cursor = i;
            var pet = thisUnit.pet;
            if (pet == null)
                continue;

            List<Buff> buffs = new List<Buff>(pet.buffController.buffs);
            buffs.Add(Buff.GetFeatureBuff(pet.info));
            buffs.Add(pet.hasEmblem ? Buff.GetEmblemBuff(pet.info) : null);

            pet.buffController.RemoveRangeBuff(x => true, null, null);
            pet.buffController.AddRangeBuff(buffs, thisUnit, state);
        }
        thisUnit.petSystem.cursor = 0;
    }
}


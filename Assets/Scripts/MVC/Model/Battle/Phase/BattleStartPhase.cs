using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStartPhase : BattlePhase
{
    public BattleStartPhase()
    {
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

    public void SetInitBuffs()
    {
        var masterInitBuff = GetInitBuffs(state.masterUnit);
        var clientInitBuff = GetInitBuffs(state.clientUnit);
        
        SetInitBuffs(state.masterUnit, masterInitBuff);
        SetInitBuffs(state.clientUnit, clientInitBuff);
        SetAchievement();

        if (state.settings.mode == BattleMode.Card)
        {
            state.masterUnit.pet.anger += 15;
        }
    }

    private void SetAchievement()
    {
        if (battle.settings.isSimulate)
            return;

        var item = Item.GetItemInfo(Player.instance.gameData.achievement);
        if (item == null)
            return;

        var handler = new EffectHandler();
        handler.AddEffects(state.masterUnit, item.effects);
        handler.CheckAndApply(state);
    }

    private List<List<Buff>> GetInitBuffs(Unit thisUnit)
    {
        List<List<Buff>> initBuffs = new List<List<Buff>>();
        for (int i = 0; i < thisUnit.petSystem.petBag.Length; i++)
        {
            thisUnit.petSystem.cursor = i;
            var pet = thisUnit.pet;
            if (pet == null)
            {
                initBuffs.Add(new List<Buff>());
                continue;
            }

            List<Buff> buffs = new List<Buff>(pet.buffController.buffs) { Buff.GetFeatureBuff(pet) };
            if (pet.hasEmblem)
                buffs.Add(Buff.GetEmblemBuff(pet));

            buffs.AddRange(pet.initBuffs);
            if (state.stateBuffs.Exists(x => x.Value.id == 600000))
                buffs.RemoveAll(x => (x != null) && x.IsType(BuffType.Item));

            initBuffs.Add(buffs);
        }
        return initBuffs;
    }

    private void SetInitBuffs(Unit thisUnit, List<List<Buff>> initBuffs)
    {
        for (int i = 0; i < thisUnit.petSystem.petBag.Length; i++)
        {
            thisUnit.petSystem.cursor = i;
            var pet = thisUnit.pet;
            if (pet == null)
                continue;

            pet.buffController.RemoveRangeBuff(initBuffs[i].Contains, null, null);
            pet.buffController.AddRangeBuff(initBuffs[i], thisUnit, state);

            pet.skillController.allSkills.Where(x => x != null).SelectMany(x => x.effects)
                .Where(x => (x != null) && (x.timing == EffectTiming.OnAddBuff)).OrderBy(x => x.priority).ToList()
                .ForEach(x => x.CheckAndApply(thisUnit, state, false, sourcePet: pet));
        }
        thisUnit.petSystem.cursor = 0;
    }
}


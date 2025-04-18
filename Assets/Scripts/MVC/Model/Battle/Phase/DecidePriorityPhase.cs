using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DecidePriorityPhase : BattlePhase
{
    public DecidePriorityPhase()
    {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnDecidePriority;
    }

    public override void DoWork()
    {
        ApplySkillsAndBuffs();
        SetActionOrder();
        state.GiveTurnToNextUnit();

        /*
        phase = EffectTiming.OnPriorityReady;
        ApplySkillsAndBuffs();
        */

        SetUIState(battle.currentState);
    }

    public override BattlePhase GetNextPhase()
    {
        return new BeforeAttackPhase();
    }

    /// <summary>
    /// Get action order list of all pets on field.
    /// </summary>
    /// <param name="priorityUnits">(Belonged unit, Used skill, Pet, Cursor of pet in the bag)</param>
    /// <returns>An action order list where each item represents (cursor + 1) of the pet and the sign represents which unit</returns>
    private List<int> GetActionOrder(List<(Unit, Skill, BattlePet, int)> priorityUnits)
    {
        return priorityUnits.OrderByDescending(p => ((p.Item2 == null) || (p.Item3 == null)) ? -1 : 0)
            .ThenByDescending(p => p.Item2.isAction ? 1 : 0)
            .ThenByDescending(p => p.Item2.priority)
            .ThenByDescending(p => p.Item3.battleStatus.spd)
            .ThenBy(p => Random.Range(0, 100))
            .Select(p => (int)(Mathf.Sign(p.Item1.id) * (p.Item4 + 1)))
            .ToList();
    }

    private void SetActionOrder() {
        var parallelCount = state.settings.parallelCount;
        var masterUnit = state.masterUnit;
        var clientUnit = state.clientUnit;
        var masterPetSystem = masterUnit.petSystem;
        var clientPetSystem = clientUnit.petSystem;
        var masterPetBag = masterPetSystem.GetParallelPetBag(parallelCount);
        var clientPetBag = clientPetSystem.GetParallelPetBag(parallelCount);

        if (parallelCount <= 1) {
            var master = (masterUnit, masterUnit.skill, masterUnit.pet, masterPetSystem.cursor);
            var client = (clientUnit, clientUnit.skill, clientUnit.pet, clientPetSystem.cursor);
            state.actionOrder = GetActionOrder(new List<(Unit, Skill, BattlePet, int)>(){ master, client });
            return;
        }

        var masterPetCursor = masterPetSystem.GetParallelPetBagCursor(parallelCount);
        var clientPetCursor = clientPetSystem.GetParallelPetBagCursor(parallelCount);
        var masterSkill = masterPetCursor.Select(cur => masterUnit.parallelSkillSystems[cur]?.skill);
        var clientSkill = clientPetCursor.Select(cur => clientUnit.parallelSkillSystems[cur]?.skill);
        var petBag = masterPetBag.Concat(clientPetBag).ToList();
        var cursor = masterPetCursor.Concat(clientPetCursor).ToList();
        var skill = masterSkill.Concat(clientSkill).ToList();
        var priorityUnits = petBag.Select((x, i) => ((i < masterPetBag.Count) ? masterUnit : clientUnit, 
            skill[i], petBag[i], cursor[i])).Where(x => (x.Item2 != null) && (x.Item3 != null) && (x.Item2.type != SkillType.空过) &&
                ((!x.Item3.isDead) || (x.Item2.isAction))).ToList();
        
        state.actionOrder = GetActionOrder(priorityUnits);
    }
}
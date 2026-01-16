using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.StructWrapping;

public class BattlePhase
{
    public Battle battle => Player.instance.currentBattle;
    public BattleManager UI => battle.UI;

    public EffectTiming phase { 
        get => (state == null) ? EffectTiming.None : state.phase; 
        set { 
            if (state != null)
                state.phase = value;
        } 
    }
    public BattleState state;
    public BattleResultState resultState;

    public virtual void DoWork() {
        ApplySkillsAndBuffs();
        SetUIState(battle.currentState);
    }

    public virtual BattlePhase GetNextPhase() {
        return null;
    }

    protected void SetUIState(BattleState lastState = null) {
        UI.SetState(lastState, state);
    }

    protected virtual void ApplySkillsAndBuffs() {
        var parallelCount = state.settings.parallelCount;
        if ((parallelCount > 1) && (state.whosTurn == 0)) {
            void ParallelApply(Unit unit, List<int> petBagCursor) {
                for (int i = 0; i < petBagCursor.Count; i++) {
                    unit.petSystem.cursor = petBagCursor[i];
                    GetEffectHandler(unit).CheckAndApply(state);
                }
                unit.petSystem.cursor = petBagCursor[0];
            }
            var masterPetBagCursor = state.masterUnit.petSystem.GetParallelPetBagCursor(parallelCount);
            var clientPetBagCursor = state.clientUnit.petSystem.GetParallelPetBagCursor(parallelCount);
            ParallelApply(state.masterUnit, masterPetBagCursor);
            ParallelApply(state.clientUnit, clientPetBagCursor);
            return;
        }

        GetEffectHandler(null).CheckAndApply(state);
    }

    protected virtual void ApplyBuffs() {
        GetEffectHandler(null, false).CheckAndApply(state);
    }

    protected virtual EffectHandler GetEffectHandler(Unit invokeUnit, bool addSkillEffect = true) {

        if (invokeUnit == null)
            return GetEffectHandler(state.masterUnit).Concat(GetEffectHandler(state.clientUnit));

        var passiveSkills = (battle.settings.mode == BattleMode.Card && invokeUnit.IsMyUnit()) ? invokeUnit.cardSystem.hand : invokeUnit.pet.skillController.allSkills;
        var skillEffects = passiveSkills.Where(x => (x != null) && (x.type == SkillType.被动))
            .Where(x => invokeUnit.IsSkillCostEnough(x.id, state.settings)).Select(x => x.effects);

        var unitEffects = invokeUnit.unitBuffs.Where(x => !x.ignore).Select(x => x.effects);
        var buffEffects = invokeUnit.pet.buffs.Where(x => !x.ignore).Select(x => x.effects);
        var handler = new EffectHandler();
        
        if (state != null) {
            handler.AddEffects(invokeUnit, state.weatherBuff.effects);

            var stateEffects = state.stateBuffs.Where(x => !x.Value.ignore).Select(x => x.Value.effects);
            foreach (var e in stateEffects)
                handler.AddEffects(invokeUnit, e);
        }

        if (addSkillEffect && (invokeUnit.skill != null))
            handler.AddEffects(invokeUnit, invokeUnit.skill.effects);

        foreach (var e in skillEffects)
            handler.AddEffects(invokeUnit, e);   

        foreach (var e in unitEffects)
            handler.AddEffects(invokeUnit, e);
        
        foreach (var e in buffEffects)
            handler.AddEffects(invokeUnit, e);

        return handler;
    }

    protected virtual bool IsAttackLegal() {
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;

        if ((atkUnit.pet == null) || (defUnit.pet == null)) {
            atkUnit.isDone = defUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        if (state.whosTurn == 0) {
            ApplySkillsAndBuffs();
            state.myUnit.isDone = state.opUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        if (atkUnit.skill.type == SkillType.道具) {
            ApplySkillsAndBuffs();
            atkUnit.isDone = true;
            atkUnit.hudSystem.OnUseItem(atkUnit, atkUnit.skill, true);
            defUnit.hudSystem.OnUseItem(defUnit, atkUnit.skill, false);
            
            SetUIState(battle.currentState);

            state = new BattleState(state);
            state.atkUnit.hudSystem.OnUseItemUndo();
            state.defUnit.hudSystem.OnUseItemUndo();
            return false;
        }

        if (atkUnit.skill.isAction) {
            ApplySkillsAndBuffs();
            atkUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        if (defUnit.pet.isDead) {
            if ((state.settings.parallelCount > 1) && (phase == EffectTiming.OnBeforeAttack)) {
                int nextAlive = defUnit.petSystem.GetNextCursorCircular();
                if (nextAlive != defUnit.petSystem.cursor) {
                    defUnit.petSystem.cursor = nextAlive;
                    SetUIState(null);
                    return true;
                }
            }

            atkUnit.SetSkill(Skill.GetNoOpSkill());
            state.atkUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        if ((atkUnit.pet.isDead) || (!atkUnit.isMovable) || (!atkUnit.IsSkillCostEnough(battle.settings.rule))
            || (atkUnit.skill.chain <= 0))
        {
            atkUnit.SetSkill(Skill.GetNoOpSkill());
            atkUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        return true;
    }

}

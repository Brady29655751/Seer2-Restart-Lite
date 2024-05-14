using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        GetEffectHandler(null).CheckAndApply(state);
    }

    protected virtual EffectHandler GetEffectHandler(Unit invokeUnit) {

        if (invokeUnit == null)
            return GetEffectHandler(state.masterUnit).Concat(GetEffectHandler(state.clientUnit));
        
        var buffEffects = invokeUnit.pet.buffs.Select(x => x.effects);
        var handler = new EffectHandler();
        
        if (state != null)
            handler.AddEffects(invokeUnit, state.weatherBuff.effects);

        if (invokeUnit.skill != null) 
            handler.AddEffects(invokeUnit, invokeUnit.skill.effects);
        
        foreach (var e in buffEffects)
            handler.AddEffects(invokeUnit, e);

        return handler;
    }

    protected virtual bool IsAttackLegal() {
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;

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
            atkUnit.SetSkill(Skill.GetNoOpSkill());
            state.atkUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        if ((atkUnit.pet.isDead) || (!atkUnit.pet.isMovable) || (atkUnit.skill.anger > atkUnit.pet.anger)) {
            atkUnit.SetSkill(Skill.GetNoOpSkill());
            atkUnit.isDone = true;
            SetUIState(null);
            return false;
        }

        return true;
    }

}

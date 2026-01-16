using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// A special phase that does not participate in battle.NextPhase()
/// </summary>
public class PassivePetChangePhase : BattlePhase
{
    public BattleState UIState;
    public BattleState recordState;

    public PassivePetChangePhase()
    {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnPassivePetChange;

        this.recordState = new BattleState(battle.currentState);
        this.UIState = new BattleState(battle.currentState);
    }

    public override void DoWork()
    {
    }

    public override BattlePhase GetNextPhase()
    {
        var masterChance = GetPassiveChance(state.masterUnit.id);
        var clientChance = GetPassiveChance(state.clientUnit.id);
        if ((masterChance > 0) || (clientChance > 0))
            return this;

        if (state.opUnit.IsMasterUnit())
        {
            UIState.masterUnit = new Unit(state.opUnit);
        }
        else
        {
            UIState.clientUnit = new Unit(state.opUnit);
        }

        UI.SetState(null, UIState);

        battle.lastState = new BattleState(recordState);
        battle.currentState = new BattleState(state);

        return new TurnReadyPhase();
    }

    public void SetSkill(Skill skill, bool isMe)
    {
        var myUnit = state.myUnit;
        var opUnit = state.opUnit;
        var changeUnit = isMe ? myUnit : opUnit;

        if (GetPassiveChance(changeUnit.id) == 0)
            return;

        changeUnit.SetSkill(skill);

        if (skill.type == SkillType.換场)
        {
            GetEffectHandler(changeUnit).CheckAndApply(state);

            if (isMe)
            {
                UIState = new BattleState(state);
                if (UIState.opUnit.IsMasterUnit())
                {
                    UIState.masterUnit = new Unit(recordState.opUnit);
                }
                else
                {
                    UIState.clientUnit = new Unit(recordState.opUnit);
                }

                UIState.myUnit.hudSystem.OnPassivePetChange(UIState.myUnit.pet);
                UI.SetState(null, UIState);
                UI.ProcessQuery(true);
                UI.SetBottomBarInteractable(GetPassiveChance(myUnit.id) > 0);
                UI.SetOptionActive(2, UIState.settings.isCaptureOK);

                if ((!UIState.settings.isItemOK) || (UIState.myUnit.pet.isDead))
                {
                    if (UIState.myUnit.pet.isDead)
                        UI.SetOptionActive(0, false);

                    UI.SetOptionActive(2, false);
                    UI.SetOptionActive(3, false);
                }
            }
        }

        if ((battle.settings.mode != BattleMode.PVP) && (GetPassiveChance(opUnit.id) != 0))
        {
            var cursor = opUnit.petSystem.cursor;
            var defaultSkill = opUnit.pet.isDead
                ? Skill.GetPetChangeSkill(cursor, opUnit.petSystem.petBag.FindIndex(x => (x != null) && !x.isDead), true)
                : opUnit.pet.GetDefaultSkill();

            SetSkill(defaultSkill, false);
        }
    }

    public int GetPassiveChance(int unitId)
    {
        var currentUnit = recordState.GetUnitById(unitId);
        var changeUnit = state.GetUnitById(unitId);

        if (changeUnit.skill == null)
            return 2;

        if (changeUnit.skill.type != SkillType.換场)
            return 0;

        if (currentUnit.petSystem.cursor != int.Parse(changeUnit.skill.options.Get("source_index", "-1")))
            return 0;

        return 1;
    }
}
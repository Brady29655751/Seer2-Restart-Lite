using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeAttackPhase : BattlePhase
{
    public BeforeAttackPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnBeforeAttack;
    }

    public override void DoWork()
    {
        if (state.isAllUnitDone)
            return;

        AddReport();

        if (!IsAttackLegal())
            return;
        
        OnChainStart();
        base.DoWork();
        CheckAccuracy();
    }

    public override BattlePhase GetNextPhase()
    {
        if (state.isAllUnitDone)
            return new AttackEndPhase();

        if (state.atkUnit.skill.isAttack && state.atkUnit.skillSystem.isHit)
            return new DamageCalculatePhase();

        return new AttackPhase();
    }

    private void OnChainStart() {
        state.atkUnit.OnChainStart();
    }

    private void CheckAccuracy() {
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        atkUnit.CalculateAccuracy(defUnit);
    }

    private void AddReport()
    {
        var atkUnit = state.atkUnit;
        var atkColor = atkUnit.IsMyUnit() ? "#ffbb33" : "#ff0080";
        var report = $"<color={atkColor}>【{atkUnit.pet.name}】</color>";

        if (atkUnit.skill.type == SkillType.空过)
            report += $"空过了本次行动，动弹不得！";
        else if (atkUnit.skill.type == SkillType.換场)
        {
            var targetIndex = (int)atkUnit.skill.GetSkillIdentifier("option[target_index]");
            var newPetName = atkUnit.petSystem.petBag.Get(targetIndex)?.name ?? "神秘精灵";
            report += $"主动更换出战精灵为<color={atkColor}>【{newPetName}】</color>";      
        }
        else if (atkUnit.skill.type == SkillType.道具)
        {
            var itemId = (int)atkUnit.skill.GetSkillIdentifier("option[item_id]");
            var item = Item.GetItemInfo(itemId);
            var itemDesc = item?.options.Get("battle_report", item?.effectDescription) ?? string.Empty;
            report += $"使用了<color=#ffff00>{atkUnit.skill.name}</color>！" + 
                itemDesc?.Replace("[atk_pet]", $"<color={atkColor}>【{atkUnit.pet.name}】</color>")
                .Replace("[atk_pet]", $"<color={atkColor}>【{atkUnit.pet.name}】</color>").ReplaceColorAndNewline();   
        }
        else if (atkUnit.skill.type == SkillType.逃跑)
            report += $"逃跑了，战斗结束！";
        else
            report = string.Empty;

        if (!string.IsNullOrEmpty(report))
            state.AddReport(report);
    }
}

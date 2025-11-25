using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackPhase : BattlePhase
{
    public AttackPhase() {
        this.state = new BattleState(battle.currentState);
        this.phase = EffectTiming.OnAttack;
    }

    public override void DoWork()
    {
        if (state.atkUnit.isDone)
            return;

        if (!IsAttackLegal())
            return;
        
        ApplySkillsAndBuffs();
        ConsumeAnger();
        OnHit();
    }

    public override BattlePhase GetNextPhase()
    {
        if (state.isAllUnitDone)
            return new AttackEndPhase();

        return new AfterAttackPhase();
    }

    private void ConsumeAnger() {
        var lastState = new BattleState(battle.currentState);
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        var hpToAngerBuff = atkUnit.pet.buffController.GetBuff(61);
        var angerDiff = atkUnit.skill.anger - atkUnit.pet.anger;

        if ((hpToAngerBuff != null) && (angerDiff > 0)) {
            atkUnit.pet.hp -= hpToAngerBuff.value * angerDiff;
            atkUnit.pet.anger += angerDiff;
        }

        atkUnit.pet.anger -= atkUnit.skill.anger;
        atkUnit.hudSystem.OnAttack(atkUnit, true);
        defUnit.hudSystem.OnAttack(defUnit, false);
        
        SetUIState(lastState);

        state = new BattleState(state);
        state.atkUnit.hudSystem.OnAttackUndo();
        state.defUnit.hudSystem.OnAttackUndo();
    }

    private void OnHit() {
        var lastState = new BattleState(state);
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        var atkColor = atkUnit.IsMyUnit() ? "#ffbb33" : "#ff0080";
        var defColor = atkUnit.IsMyUnit() ? "#ff0080" : "#ffbb33";

        var report = string.Empty;

        if (atkUnit.skill.GetSkillIdentifier("evolve") != 0)
            state.AddReport($"<color={atkColor}>【{atkUnit.pet.name}】</color>激活了专属动作！");

        if (atkUnit.skill.type >= SkillType.属性)
            report += $"<color={atkColor}>【{atkUnit.pet.name}】</color>使用了<color=#00ffff>{atkUnit.skill.name}</color>，";

        if (atkUnit.skill.type == SkillType.属性)
            report += "是属性技能！";

        if (atkUnit.skillSystem.isHit) 
        {
            defUnit.pet.hp -= atkUnit.skillSystem.totalSkillDamage;
            defUnit.pet.anger += Mathf.FloorToInt((defUnit.pet.maxAnger / 2 - 1) * (1f * atkUnit.skillSystem.totalSkillDamage / defUnit.pet.maxHp) * (defUnit.pet.battleStatus.angrec / 100f));

            if (atkUnit.skill.isAttack && atkUnit.skillSystem.isCritical)
                report += "打出了致命一击，";
            
            if (atkUnit.skill.isAttack || (atkUnit.skillSystem.totalSkillDamage > 0))
                report += $"對<color={defColor}>【{defUnit.pet.name}】</color>造成了 <color=#ff4444><size=13>{atkUnit.skillSystem.totalSkillDamage}</size></color> 點傷害！";   

            if (atkUnit.skill.options.TryGet("battle_report", out var customReport)) 
                report += customReport.Replace("[atk_pet]", $"<color={atkColor}>【{atkUnit.pet.name}】</color>")
                    .Replace("[def_pet]", $"<color={defColor}>【{defUnit.pet.name}】</color>").ReplaceColorAndNewline();
            /*
            else
            {
                var abnormalBuff = atkUnit.pet.buffController.GetRangeBuff(x => x.IsType(BuffType.Abnormal)).FirstOrDefault();
                var unhealthyBuff = atkUnit.pet.buffController.GetRangeBuff(x => x.IsType(BuffType.Unhealthy)).FirstOrDefault();
                if (abnormalBuff == null && unhealthyBuff == null)
                    report += "<color=#00ffff>【状态】正常</color>";
                else
                    report += $"<color=#00ffff>【状态】{abnormalBuff?.name} {unhealthyBuff?.name}</color>";
            }
            */
        } 
        else
        {
            report += $"但是没有命中<color={defColor}>【{defUnit.pet.name}】</color>！";
        }

        state.AddReport(report);

        atkUnit.hudSystem.OnHit(atkUnit, true);
        defUnit.hudSystem.OnHit(defUnit, false);

        SetUIState(lastState);

        state = new BattleState(state);
        atkUnit = state.atkUnit;
        defUnit = state.defUnit;

        atkUnit.hudSystem.OnHitUndo(true);
        defUnit.hudSystem.OnHitUndo(false);
        
    }

}

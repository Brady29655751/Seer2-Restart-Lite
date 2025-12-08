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
        AddReport();
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

    private void OnHit() 
    {
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;
        
        var pinkDamage = atkUnit.skillSystem.GetTypeDamage(type => (type == "skill_fix") || (type == "skill_per")); // 粉傷 (固傷+百分比)
        var whiteDamage = atkUnit.skillSystem.GetTypeDamage(type => type == "skill_act"); // 白傷 (真實傷害)

        var pinkHeal = atkUnit.skillSystem.GetTypeHeal(type => (type == "skill_fix") || (type == "skill_per")); // 粉血 (固回復+百分比)
        var whiteHeal = atkUnit.skillSystem.GetTypeHeal(type => type == "skill_act"); // 白血 (真實回復)

        ApplyAttackDamage(pinkDamage + whiteDamage, pinkHeal + whiteHeal);

        atkUnit = state.atkUnit;
        defUnit = state.defUnit;

        ApplySpecialHit("pink", pinkDamage, pinkHeal);
        ApplySpecialHit("white", whiteDamage, whiteHeal);
    }

    private void ApplyAttackDamage(int exceptDamage, int exceptHeal)
    {
        var lastState = new BattleState(state);
        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;

        var redDamage = atkUnit.skillSystem.totalSkillDamage - exceptDamage; // 紅傷 (純攻擊傷害)
        var redHeal = atkUnit.skillSystem.heal - exceptHeal;  // 紅血 (純回復)

        if (atkUnit.skillSystem.isHit)
        {
            defUnit.pet.hp -= redDamage;
            defUnit.pet.anger += Mathf.FloorToInt((defUnit.pet.maxAnger / 2 - 1) * (1f * redDamage / defUnit.pet.maxHp) * (defUnit.pet.battleStatus.angrec / 100f));   
        }

        atkUnit.hudSystem.OnHit(atkUnit, true, redDamage, redHeal);
        defUnit.hudSystem.OnHit(defUnit, false, 0, 0);

        SetUIState(lastState);

        state = new BattleState(state);
        atkUnit = state.atkUnit;
        defUnit = state.defUnit;

        atkUnit.hudSystem.OnHitUndo(true);
        defUnit.hudSystem.OnHitUndo(false);
    }

    private void ApplySpecialHit(string colorType, int damage, int heal)
    {
        if ((damage <= 0) && (heal == 0))
            return;

        var atkUnit = state.atkUnit;
        var defUnit = state.defUnit;

        if (damage > 0)
        {
            defUnit.pet.hp -= damage;
            defUnit.pet.anger += Mathf.FloorToInt((defUnit.pet.maxAnger / 2 - 1) * (1f * damage / defUnit.pet.maxHp) * (defUnit.pet.battleStatus.angrec / 100f));   
            defUnit.hudSystem.CurHealInfo = new UnitHudSystem.HealInfo(-damage, defUnit.IsMyUnit(), colorType);
        }
            
        if (heal != 0)
            atkUnit.hudSystem.CurHealInfo = new UnitHudSystem.HealInfo(heal, atkUnit.IsMyUnit(), colorType);

        SetUIState(new BattleState(state));

        state = new BattleState(state);
        atkUnit = state.atkUnit;
        defUnit = state.defUnit;

        atkUnit.hudSystem.OnHitUndo(true);
        defUnit.hudSystem.OnHitUndo(false);
    }

    private void AddReport() 
    {
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
            if (atkUnit.skill.isAttack && atkUnit.skillSystem.isCritical)
                report += "打出了致命一击，";
        } 
        else
        {
            report += $"但是没有命中<color={defColor}>【{defUnit.pet.name}】</color>！";
        }

        if (atkUnit.skillSystem.totalSkillDamage > 0)
        {
            report += $"對<color={defColor}>【{defUnit.pet.name}】</color>总共造成了 <color=#ff4444><size=13>{atkUnit.skillSystem.totalSkillDamage}</size></color> 點傷害！";   

            var damageTypes = new Dictionary<string, string>
            {
                {"skill_fix", "固定"},
                {"skill_per", "百分比"},
                {"skill_act", "真实"},
            };

            foreach (var entry in damageTypes)
            {
                if (atkUnit.skillSystem.damageDict.TryGet(entry.Key, out var damage) && (damage > 0))
                {
                    report += $"<color=#ff8cff>【{entry.Value}伤害】<size=13>{damage}</size=13></color>";
                }
            }
        }

        if (atkUnit.skill.options.TryGet("battle_report", out var customReport)) 
            report += "  " + customReport.Replace("[atk_pet]", $"<color={atkColor}>【{atkUnit.pet.name}】</color>")
                .Replace("[def_pet]", $"<color={defColor}>【{defUnit.pet.name}】</color>").ReplaceColorAndNewline();

        state.AddReport(report);
    }

}

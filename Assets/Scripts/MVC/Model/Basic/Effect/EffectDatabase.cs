using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EffectDatabase {

    private static Dictionary<string, EffectTiming> timingConvDict = new Dictionary<string, EffectTiming>() {    
        {"all", EffectTiming.All},
        {"none", EffectTiming.None},
        {"resident", EffectTiming.Resident},
        {"on_battle_start", EffectTiming.OnBattleStart},
        {"on_turn_start", EffectTiming.OnTurnStart},
        {"on_turn_ready", EffectTiming.OnTurnReady},
        {"on_decide_pri", EffectTiming.OnDecidePriority},
        {"on_before_attack", EffectTiming.OnBeforeAttack},
        {"on_attack_start", EffectTiming.OnAttackStart},
        {"on_dmg_param_cal", EffectTiming.OnDamageParamCalculate},
        {"on_before_dmg_cal", EffectTiming.OnBeforeDamageCalculate},
        {"on_dmg_cal", EffectTiming.OnDamageCalculate},
        {"on_after_dmg_cal", EffectTiming.OnAfterDamageCalculate},
        {"on_final_dmg_cal", EffectTiming.OnFinalDamageCalculate},
        {"on_attack_ready", EffectTiming.OnAttackReady},
        {"on_attack", EffectTiming.OnAttack},
        {"on_after_attack", EffectTiming.OnAfterAttack},
        {"on_attack_end", EffectTiming.OnAttackEnd},
        {"on_turn_end", EffectTiming.OnTurnEnd},
        {"on_after_turn_end", EffectTiming.OnAfterTurnEnd},
        {"on_battle_end", EffectTiming.OnBattleEnd},

        {"on_passive_pet_change", EffectTiming.OnPassivePetChange},
        {"on_select_target", EffectTiming.OnSelectTarget},
        {"on_add_buff", EffectTiming.OnAddBuff},
        {"on_remove_buff", EffectTiming.OnRemoveBuff},
        {"on_before_pet_change", EffectTiming.OnBeforePetChange},
        {"on_after_pet_change", EffectTiming.OnAfterPetChange},
        {"on_round_start", EffectTiming.OnRoundStart},
        {"on_round_end", EffectTiming.OnRoundEnd},
    };

    private static Dictionary<string, EffectTarget> targetConvDict = new Dictionary<string, EffectTarget>() {
        {"none", EffectTarget.None},
        {"state", EffectTarget.CurrentState},
        {"pet", EffectTarget.CurrentPet},
        {"pet_bag", EffectTarget.CurrentPetBag},
        {"token", EffectTarget.CurrentToken},
        {"skill", EffectTarget.CurrentSkill},
        {"item", EffectTarget.CurrentItem},
        {"buff",EffectTarget.CurrentBuff},
    };

    private static Dictionary<string, EffectCondition> condConvDict = new Dictionary<string, EffectCondition>() {
        {"none", EffectCondition.None},
        {"unit", EffectCondition.CurrentUnit},
        {"pet", EffectCondition.CurrentPet},
        {"token", EffectCondition.CurrentToken},
        {"status", EffectCondition.CurrentStatus},
        {"buff", EffectCondition.CurrentBuff},
        {"skill", EffectCondition.CurrentSkill},
        {"last_skill", EffectCondition.LastSkill},
        {"poker", EffectCondition.Poker},
    };

    private static Dictionary<string, EffectAbility> typeConvDict = new Dictionary<string, EffectAbility>() {
        {"none", EffectAbility.None},
        {"win", EffectAbility.Win},
        {"escape", EffectAbility.Escape},
        {"capture", EffectAbility.Capture},
        {"pet_change", EffectAbility.PetChange},
        {"heal", EffectAbility.Heal},
        {"rage", EffectAbility.Rage},
        {"powerup", EffectAbility.Powerup},
        {"add_status", EffectAbility.AddStatus},
        {"block_buff", EffectAbility.BlockBuff},
        {"add_buff", EffectAbility.AddBuff},
        {"remove_buff", EffectAbility.RemoveBuff},
        {"copy_buff", EffectAbility.CopyBuff},
        {"set_buff", EffectAbility.SetBuff},
        {"set_damage", EffectAbility.SetDamage},
        {"set_skill", EffectAbility.SetSkill},
        {"set_pet", EffectAbility.SetPet},
        {"set_weather", EffectAbility.SetWeather},
        {"set_player", EffectAbility.SetPlayer},
        {"poker", EffectAbility.Poker},
        {"set_pet_bag", EffectAbility.SetPetBag},
        {"card", EffectAbility.Card},
    };

    public static EffectTiming ToEffectTiming(this string timing) {
        return timingConvDict.Get(timing, EffectTiming.None);
    }

    public static string ToRawString(this EffectTiming timing) {
        return timingConvDict.ContainsValue(timing) ? timingConvDict.First(x => x.Value == timing).Key : "none";
    }

    public static EffectTarget ToEffectTarget(this string target) {
        return targetConvDict.Get(target, EffectTarget.None);
    }

    public static string ToRawString(this EffectTarget target) {
        return targetConvDict.ContainsValue(target) ? targetConvDict.First(x => x.Value == target).Key : "none";
    }

    public static EffectCondition ToEffectCondition(this string condition) {
        return condConvDict.Get(condition, EffectCondition.None);
    }

    public static string ToRawString(this EffectCondition condition) {
        return condConvDict.ContainsValue(condition) ? condConvDict.First(x => x.Value == condition).Key : "none";
    }

    public static EffectAbility ToEffectAbility(this string ability) {
        return typeConvDict.Get(ability, EffectAbility.None);
    }

    public static string ToRawString(this EffectAbility ability) {
        return typeConvDict.ContainsValue(ability) ? typeConvDict.First(x => x.Value == ability).Key : "none";
    }

    public static bool IsAttackPhase(this EffectTiming timing)
    {
        bool isAttackPhase = (timing > EffectTiming.OnBeforeAttack) && (timing < EffectTiming.OnAttackEnd);
        bool isDamagePhase = (timing >= EffectTiming.OnDamageParamCalculate) && (timing <= EffectTiming.OnFinalDamageCalculate);
        return isAttackPhase && (!isDamagePhase);
    }
}

public enum EffectTiming {
    All = -999,
    None = 0, Resident = 1, OnBattleStart = 2,
    OnTurnStart = 3, OnTurnReady = 4, OnDecidePriority = 5, OnBeforeAttack = 6, OnAttackStart = 7,
    OnDamageParamCalculate = 8, OnBeforeDamageCalculate = 9, OnDamageCalculate = 10, OnAfterDamageCalculate = 11, OnFinalDamageCalculate = 12,
    OnAttackReady = 13, OnAttack = 14, OnAfterAttack = 15, OnAttackEnd = 16, OnTurnEnd = 17, OnAfterTurnEnd = 18, OnBattleEnd = 999,

    OnPassivePetChange = -1,    OnSelectTarget = -2,    OnAddBuff = -3,     OnRemoveBuff = -4,
    OnBeforePetChange = -5,     OnAfterPetChange = -6,  OnRoundStart =-7,   OnRoundEnd = -8,
}

public enum EffectTarget
{
    None,
    CurrentState,
    CurrentItem,
    CurrentSkill,
    CurrentBuff,
    CurrentPet,
    CurrentPetBag,
    CurrentToken,
}

public enum EffectCondition {
    None,
    CurrentUnit,
    CurrentPet,
    CurrentToken,
    CurrentStatus,
    CurrentSkill,
    CurrentBuff,
    LastSkill,
    Poker,
}

public enum EffectAbility
{
    None,
    Win,
    Escape,
    Capture,
    PetChange,
    Heal,
    Rage,
    Powerup,
    AddStatus,
    BlockBuff,
    AddBuff,
    RemoveBuff,
    CopyBuff,
    SetBuff,
    SetDamage,
    SetSkill,
    SetPet,
    SetWeather,
    SetPlayer,
    Poker,
    SetPetBag,
    Card,
}
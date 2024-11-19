using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EffectDatabase {

    private static Dictionary<string, EffectTiming> timingConvDict = new Dictionary<string, EffectTiming>() {
        {"on_after_pet_change", EffectTiming.OnAfterPetChange},
        {"on_before_pet_change", EffectTiming.OnBeforePetChange},
        {"on_remove_buff", EffectTiming.OnRemoveBuff},
        {"on_add_buff", EffectTiming.OnAddBuff},
        {"on_passive_pet_change", EffectTiming.OnPassivePetChange},
        
        {"none", EffectTiming.None},
        {"resident", EffectTiming.Resident},
        {"on_battle_start", EffectTiming.OnBattleStart},
        {"on_turn_start", EffectTiming.OnTurnStart},
        {"on_turn_ready", EffectTiming.OnTurnReady},
        {"on_decide_pri", EffectTiming.OnDecidePriority},
        {"on_pri_ready", EffectTiming.OnPriorityReady},
        {"on_before_attack", EffectTiming.OnBeforeAttack},
        {"on_before_dmg_cal", EffectTiming.OnBeforeDamageCalculate},
        {"on_dmg_cal", EffectTiming.OnDamageCalculate},
        {"on_after_dmg_cal", EffectTiming.OnAfterDamageCalculate},
        {"on_final_dmg_cal", EffectTiming.OnFinalDamageCalculate},
        {"on_attack", EffectTiming.OnAttack},
        {"on_after_attack", EffectTiming.OnAfterAttack},
        {"on_turn_end", EffectTiming.OnTurnEnd},
        {"on_battle_end", EffectTiming.OnBattleEnd},
    };

    private static Dictionary<string, EffectTarget> targetConvDict = new Dictionary<string, EffectTarget>() {
        {"none", EffectTarget.None},
        {"state", EffectTarget.CurrentState},
        {"pet", EffectTarget.CurrentPet},
        {"pet_bag", EffectTarget.CurrentPetBag},
        {"skill", EffectTarget.CurrentSkill},
        {"item", EffectTarget.CurrentItem},
        {"buff",EffectTarget.CurrentBuff},
    };

    private static Dictionary<string, EffectCondition> condConvDict = new Dictionary<string, EffectCondition>() {
        {"none", EffectCondition.None},
        {"unit", EffectCondition.CurrentUnit},
        {"pet", EffectCondition.CurrentPet},
        {"status", EffectCondition.CurrentStatus},
        {"buff", EffectCondition.CurrentBuff},
        {"skill", EffectCondition.CurrentSkill},
        {"last_skill", EffectCondition.LastSkill},
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
}

public enum EffectTiming {
    OnBeforePetChange = -4, OnAfterPetChange = -5,
    OnAddBuff = -2, OnRemoveBuff = -3,
    OnPassivePetChange = -1,

    None = 0, Resident = 1, OnBattleStart = 2,
    OnTurnStart = 3, OnTurnReady = 4, OnDecidePriority = 5, OnPriorityReady = 6, OnBeforeAttack = 7,
    OnBeforeDamageCalculate = 8, OnDamageCalculate = 9, OnAfterDamageCalculate = 10, OnFinalDamageCalculate = 11,
    OnAttack = 12, OnAfterAttack = 13, OnTurnEnd = 14, 
    OnBattleEnd = 999,
}

public enum EffectTarget {
    None,
    CurrentState,
    CurrentItem,
    CurrentSkill,
    CurrentBuff,
    CurrentPet,
    CurrentPetBag,
}

public enum EffectCondition {
    None,
    CurrentUnit,
    CurrentPet,
    CurrentStatus,
    CurrentSkill,
    CurrentBuff,
    LastSkill,
}

public enum EffectAbility {
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
}
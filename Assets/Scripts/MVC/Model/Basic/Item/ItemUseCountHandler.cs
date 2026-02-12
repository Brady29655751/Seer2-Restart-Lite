using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemUseCountHandler
{
    public static int HpPotionMaxUseCount(this Item item, object invokeUnit, BattleState state) {
        string value = item.effects.Find(x => x.ability == EffectAbility.Heal)?.abilityOptionDict.Get("add", "0");
        if (string.IsNullOrEmpty(value) || (value == "0"))
            return 1;

        float heal = float.Parse(value);
        if (state == null) {
            Pet pet = (Pet)invokeUnit;
            return Mathf.CeilToInt((pet.normalStatus.hp - pet.currentStatus.hp) / heal);
        }
        return 1;
    }

    public static int ExpMaxUseCount(this Item item, object invokeUnit, BattleState state) {
        string value = item.effects.Find(x => x.ability == EffectAbility.SetPet).abilityOptionDict.Get("value", "0");
        Pet pet = (Pet)invokeUnit;
        float exp = pet.TryGetPetIdentifier(value, out exp) ? exp : Identifier.GetNumIdentifier(value);
        return Mathf.CeilToInt((pet.maxExp - pet.totalExp) / exp);
    }

    public static int EvMaxUseCount(this Item item, object invokeUnit, BattleState state) {
        var abilityOptionDict = item.effects.Find(x => x.ability == EffectAbility.SetPet).abilityOptionDict;
        string type = abilityOptionDict.Get("type", "evStorage");
        string value = abilityOptionDict.Get("value", "0");
        Pet pet = (Pet)invokeUnit;
        float ev = float.Parse(value);

        if (type == "evReset")
            return 1;

        return Mathf.CeilToInt((510 - pet.talent.ev.sum - pet.talent.evStorage) / ev);
    }

    public static int StuffMaxUseCount(this Item item, object invokeUnit, BattleState state) {
        if (ListHelper.IsNullOrEmpty(item.effects))
            return int.MaxValue;

        if (state == null)
            return item.effects.Min(x => Mathf.CeilToInt(Parser.ParseEffectOperation(x.abilityOptionDict.Get("max_use", "1"), x, null, null, invokeUnit, false)));
        
        Unit lhsUnit = (Unit)invokeUnit;
        Unit rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        return item.effects.Min(x => Mathf.CeilToInt(Parser.ParseEffectOperation(x.abilityOptionDict.Get("max_use", "1"), x, lhsUnit, rhsUnit)));
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemEffectHandler
{
    public static int UseDefault(this Item item, object invokeUnit, BattleState state, int useCount = 1) {
        int maxCount = item.GetMaxUseCount(invokeUnit, state);
        int count = Mathf.Min(item.num, useCount, maxCount);
        var handler = new EffectHandler();
        for (int i = 0; i < count; i++) {
            handler.AddEffects(invokeUnit, item.effects);
        }
        handler.CheckAndApply(state);
        return count;
    }

    public static int UseExp(this Item item, object invokeUnit, BattleState state, int useCount = 1) {
        string value = item.effects.Find(x => x.ability == EffectAbility.SetPet).abilityOptionDict.Get("value", "0");
        Pet pet = (Pet)invokeUnit;
        float exp = pet.TryGetPetIdentifier(value, out exp) ? exp : Identifier.GetNumIdentifier(value);
        int maxCount = Mathf.CeilToInt((pet.maxExp - pet.totalExp) / exp);
        int count = Mathf.Min(item.num, useCount, maxCount);
        pet.GainExp((uint)(exp * count));
        return count;
    }
}

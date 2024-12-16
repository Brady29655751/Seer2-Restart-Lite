using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    public static Dictionary<string, ItemType> typeConvDict = new Dictionary<string, ItemType>() {
        {"none", ItemType.None},
        {"currency", ItemType.Currency},
        {"hp_potion", ItemType.HpPotion},
        {"capture", ItemType.Capture},
        {"evolve", ItemType.Evolve},
        {"emblem", ItemType.Emblem},
        {"exp", ItemType.EXP},
        {"ev", ItemType.EV},
        {"iv", ItemType.IV},
        {"personality", ItemType.Personality},
        {"skill", ItemType.Skill},
        {"buff", ItemType.Buff},
        {"mine", ItemType.Mine},
        {"recipe", ItemType.Recipe},
        {"stuff", ItemType.Stuff},
        {"package", ItemType.Package},
        {"plant", ItemType.Plant},
        {"seed", ItemType.Seed},
    };

    public static Dictionary<ItemCategory, List<ItemType>> categoryDict = new Dictionary<ItemCategory, List<ItemType>>() {
        { ItemCategory.Currency, new List<ItemType>() { ItemType.Currency } },
        { ItemCategory.Pet, new List<ItemType>() { ItemType.HpPotion, ItemType.Evolve, ItemType.Emblem, ItemType.EXP, ItemType.EV,
            ItemType.IV, ItemType.Personality, ItemType.Skill, ItemType.Buff, ItemType.Package } },
        { ItemCategory.Battle, new List<ItemType>() { ItemType.HpPotion } },
        { ItemCategory.Capture, new List<ItemType>() { ItemType.Capture } },
        { ItemCategory.Stuff, new List<ItemType>() { ItemType.Mine, ItemType.Stuff }},
        { ItemCategory.Plant, new List<ItemType>() { ItemType.Plant, ItemType.Seed }},

        { ItemCategory.PetGrow, new List<ItemType>() { ItemType.Evolve, ItemType.EXP, ItemType.EV, ItemType.IV, ItemType.Personality } },
        { ItemCategory.PetEmblem, new List<ItemType>() { ItemType.Emblem } },
        { ItemCategory.PetBattle, new List<ItemType>() { ItemType.HpPotion, ItemType.Skill, ItemType.Buff } },
        { ItemCategory.PetOthers, new List<ItemType>() { ItemType.Package } },
    };

    public static ItemType ToItemType(this string type) {
        return typeConvDict.Get(type, ItemType.None);
    }

    public static string ToRawString(this ItemType type) {
        return typeConvDict.ContainsValue(type) ? typeConvDict.First(x => x.Value == type).Key : "none";
    }

    public static bool IsInCategory(this ItemType type, ItemCategory category) {
        if (category == ItemCategory.All)
            return (type != ItemType.Currency);

        var typeList = categoryDict.Get(category, null);
        return (typeList == null) ? false : typeList.Contains(type);
    }

}

public enum ItemType {
    None,
    Currency,
    HpPotion,
    Capture,
    Evolve,
    Emblem,
    EXP,
    EV,
    IV,
    Personality,
    Skill,
    Buff,
    Mine,
    Recipe,
    Stuff,
    Package,
    Plant,
    Seed,
}

public enum ItemCategory {
    All,
    Currency,
    Pet,
    Battle,
    Capture,
    Stuff,
    PetGrow,
    PetEmblem,
    PetBattle,
    PetOthers,
    Plant,
}

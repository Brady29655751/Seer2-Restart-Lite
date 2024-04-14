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
        {"exp", ItemType.EXP},
        {"ev", ItemType.EV},
        {"iv", ItemType.IV},
        {"personality", ItemType.Personality},
        {"mine", ItemType.Mine},
    };

    public static Dictionary<ItemCategory, List<ItemType>> categoryDict = new Dictionary<ItemCategory, List<ItemType>>() {
        { ItemCategory.Currency, new List<ItemType>() { ItemType.Currency } },
        { ItemCategory.Pet, new List<ItemType>() { ItemType.HpPotion, ItemType.EXP, ItemType.EV, ItemType.IV, ItemType.Personality } },
        { ItemCategory.Battle, new List<ItemType>() { ItemType.HpPotion } },
        { ItemCategory.Capture, new List<ItemType>() { ItemType.Capture } },
        { ItemCategory.Stuff, new List<ItemType>() { ItemType.Mine }},
    };

    public static ItemType ToItemType(this string type) {
        return typeConvDict.Get(type, ItemType.None);
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
    EXP,
    EV,
    IV,
    Personality,
    Mine,
}

public enum ItemCategory {
    All,
    Currency,
    Pet,
    Battle,
    Capture,
    Stuff,
}

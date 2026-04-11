using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetIVSystem
{
    public static Dictionary<int, string> genderNameDict => new Dictionary<int, string>()
    {
        { -1, "无性" },
        { 0, "雄性" },
        { 1, "雌性" },
        { 2, "双性" },
    };

    public static IVRanking GetIVRank(int iv)
    {
        if (iv < 0)     return IVRanking.Bad;
        if (iv < 11)    return IVRanking.Normal;
        if (iv < 21)    return IVRanking.Good;
        if (iv < 28)    return IVRanking.Great;
        if (iv < 31)    return IVRanking.Rare;
        if (iv == 31)   return IVRanking.Perfect;

        return IVRanking.Cheat;
    }

    public static string GetGenderName(int gender)
    {
        return genderNameDict.Get(gender, "未知性别");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetIVSystem
{
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
}

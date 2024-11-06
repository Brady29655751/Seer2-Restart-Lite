using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetExpSystem
{
    private static Func<int, uint> n3 = (n) => (uint)(n * (n + 1) * (2 * n + 1) / 6);
    private static Func<int, uint> n2 = (n) => (uint)(n * (n + 1) / 2);
    private static Func<int, uint> n1 = (n) => (uint)(n);

    #region exp-system

    public static uint GetLevelExp(int level, int expType = 0) {
        return (level == 0) ? 0 : GetLevelExpFormula(expType)(level);
    }

    public static uint GetTotalExp(int level, int expType = 0) {
        return (level == 0) ? 0 : GetTotalExpFormula(expType)(level-1);
    }

    private static Func<int, uint> GetLevelExpFormula(int expType) {
        switch (expType) {
            default:
                return LevelExpTypeA;
        }
    }

    private static Func<int, uint> GetTotalExpFormula(int expType) {
        switch (expType) {
            default:
                return TotalExpTypeA;
        }
    }

    private static uint LevelExpBasic(int level, Vector3 v) {
        return (uint)(v.x * level * level + v.y * level + v.z);
    }

    private static uint TotalExpBasic(int level, Vector4 q) {
        return (uint)(q.x * n3(level) + q.y * n2(level) + q.z * n1(level) + q.w);
    }
    /// <summary>
    /// 【Type A】 <br/>
    /// Total: 125w <br/>
    /// Lv1 Exp: 10 <br/>
    /// Lv0 Exp (Constant): 1
    /// </summary>
    private static uint LevelExpTypeA(int level) {
        Vector3 a = new Vector3(3.75f, 3.75f, 1);
        uint exp = LevelExpBasic(level, a) + (uint)((level % 2 == 1) ? 1 : 0);
        return (level == 1) ? exp + 1 : exp;
    }
    private static uint TotalExpTypeA(int level) {
        Vector4 a =  new Vector4(3.75f, 3.75f, 1, 1.5f);
        uint exp = TotalExpBasic(level, a) + n1((level + 1) / 4);
        return exp;
    }

    #endregion

    #region evolve-system

    public static List<int> GetEvolveChain(int baseId, int id) {
        List<int> chain = new List<int>(){ baseId };

        if (baseId == id)
            return chain;

        var info = Pet.GetPetInfo(baseId);
        var evolveIds = info.exp.evolvePetIds;

        if (ListHelper.IsNullOrEmpty(evolveIds) || (evolveIds.All(x => x == 0)))
            return null;

        for (int i = 0; i < evolveIds.Count; i++) {
            var nextChain = GetEvolveChain(evolveIds[i], id);
            if (nextChain != null) {
                chain.AddRange(nextChain);
                return chain;
            }
        }

        return null;
    }

    #endregion
}
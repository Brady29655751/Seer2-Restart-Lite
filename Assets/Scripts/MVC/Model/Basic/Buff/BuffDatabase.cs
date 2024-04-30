using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuffDatabase
{   
    public static Dictionary<string, BuffType> typeConvDict = new Dictionary<string, BuffType>() {
        {"none", BuffType.None},
        {"feature", BuffType.Feature},
        {"emblem", BuffType.Emblem},
        {"mark", BuffType.Mark},
        {"unhealthy", BuffType.Unhealthy},
        {"abnormal", BuffType.Abnormal},
        {"turn", BuffType.TurnBased},
        {"chance", BuffType.ChanceBased},
        {"shield", BuffType.Shield},
    };

    public static Dictionary<string, CopyHandleType> copyConvDict = new Dictionary<string, CopyHandleType>() {
        {"new", CopyHandleType.New},
        {"block", CopyHandleType.Block},
        {"replace", CopyHandleType.Replace},
        {"stack", CopyHandleType.Stack},
        {"max", CopyHandleType.Max},
        {"min", CopyHandleType.Min},
    };

    public static BuffType ToBuffType(this string type) {
        return typeConvDict.Get(type, BuffType.None);
    }

    public static string ToRawString(this BuffType type) {
        return typeConvDict.ContainsValue(type) ? typeConvDict.First(x => x.Value == type).Key : "none";
    }

    public static CopyHandleType ToCopyHandleType(this string type) {
        return copyConvDict.Get(type, CopyHandleType.New);
    }

    public static string ToRawString(this CopyHandleType type) {
        return copyConvDict.ContainsValue(type) ? copyConvDict.First(x => x.Value == type).Key : "none";
    }
}

public enum BuffType {
    None = 0,       // 無
    Feature = 1,    // 特性類狀態
    Emblem = 2,     // 紋章類狀態
    Mark = 3,       // 標記類狀態（僅為顯示用）
    Unhealthy = 4,  // 不良狀態
    Abnormal = 5,   // 異常狀態
    TurnBased = 6,  // 回合類狀態
    ChanceBased = 7,// 次數類狀態 
    Shield = 8,     // 護盾類狀態
}

public enum CopyHandleType {
    New,
    Block,
    Replace,
    Stack,
    Min,
    Max,
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Operator {
    public static Dictionary<string, Func<float, float, bool>> condDict { get; } = new Dictionary<string, Func<float, float, bool>>() {
        {"<", LessThan},
        {">", GreaterThan},
        {"=", Equal},
        {"LTE", LessThanOrEqual},
        {"GTE", GreaterThanOrEqual},
        {"NOT", NotEqual}
    };

    public static string ToHalfCondition(this string str) {
        return str.Replace("＜", "<").Replace("＞", ">").Replace("＝", "=");
    }

    public static KeyValuePair<string, string> SplitCondition(this string str, out string op, string defaultOp = "=", bool toHalf = true) {
        var halfStr = toHalf ? str.ToHalfCondition() : str;
        var opIndex = -1;

        op = defaultOp;
    
        foreach (var key in Operator.condDict.Keys) {
            opIndex = halfStr.IndexOf(key);
            if (opIndex != -1) {
                op = key;
                break;
            }
        }
        string type = halfStr.Substring(0, opIndex);
        string value = halfStr.Substring(opIndex + op.Length);
        return new KeyValuePair<string, string>(type, value);
    }

    public static bool Condition(string op, float lhs, float rhs) {
        return condDict.ContainsKey(op) ? condDict.Get(op).Invoke(lhs, rhs) : false;
    }

    public static bool Condition(string op, float current, float max, KeyValuePair<DataType, object> data) {
        var type = data.Key;
        var value = data.Value;

        switch (type) {
            default:
                return false;
            case DataType.Int:
                return Condition(op, current, (int)value);
            case DataType.Float:
                return Condition(op, current, (float)value);
            case DataType.Fraction:
                var frac = (KeyValuePair<int, int>)value;
                return Condition(op, current, max * frac.Key / frac.Value);
        }
    }

    public static bool LessThan(float lhs, float rhs) {
        return lhs < rhs;
    }

    public static bool GreaterThan(float lhs, float rhs) {
        return lhs > rhs;
    }

    public static bool Equal(float lhs, float rhs) {
        return lhs == rhs;
    }

    public static bool LessThanOrEqual(float lhs, float rhs) {
        return LessThan(lhs, rhs) || Equal(lhs, rhs);
    }

    public static bool GreaterThanOrEqual(float lhs, float rhs) {
        return GreaterThan(lhs, rhs) || Equal(lhs, rhs);
    }

    public static bool NotEqual(float lhs, float rhs) {
        return lhs != rhs;
    }

    public static Dictionary<string, Func<float, float, float>> opDict { get; } = new Dictionary<string, Func<float, float, float>>() {
        {"+", Add},  {"-", Sub},  {"*", Mult},  {"/", Div},  {"^", Pow},  {"%", Mod},  
        {"[MIN]", Mathf.Min},     {"[MAX]", Mathf.Max},      {"SET", Set}, 
    };

    public static float Operate(string op, float lhs, float rhs) {
        return opDict.ContainsKey(op) ? opDict.Get(op).Invoke(lhs, rhs) : 0;
    }

    public static float Add(float lhs, float rhs) {
        return lhs + rhs;
    }
    public static float Sub(float lhs, float rhs) {
        return lhs - rhs;
    }
    public static float Mult(float lhs, float rhs) {
        return lhs * rhs;
    }
    public static float Div(float lhs, float rhs) {
        return lhs / rhs;
    }
    public static float Pow(float lhs, float rhs) {
        return Mathf.Pow(lhs, rhs);
    }
    public static float Mod(float lhs, float rhs) {
        return (int)lhs % (int)rhs;
    }
    public static float Set(float lhs, float rhs) {
        return rhs;
    }

}

public enum DataType {
    Null,
    Text,
    Condition,
    Operator,
    Int,
    Float,
    Fraction,
}

public enum ModifyOption {
    Clear,
    Set,
    Add,
    Remove,
}
using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKeyValuePair<TKey, TValue>
{
    [XmlElement("key")] public TKey key;
    [XmlElement("value")] public TValue value;

    public IKeyValuePair() { }
    public IKeyValuePair(TKey k, TValue v)
    {
        key = k;
        value = v;
    }

    public IKeyValuePair(IKeyValuePair<TKey, TValue> rhs)
    {
        key = rhs.key;
        value = rhs.value;
    }

    public static List<IKeyValuePair<TKey, TValue>> Parse(string text, Func<string, TKey> keyFunc, Func<string, TValue> valueFunc, TValue defaultValue = default(TValue), string delimeter = "/")
    {
        var items = new List<IKeyValuePair<TKey, TValue>>();
        if (ListHelper.IsNullOrEmpty(text))
            return items;

        var split = text.Split(delimeter);
        for (int i = 0; i < split.Length; i++)
        {
            var itemExpr = split[i];
            var index = itemExpr.IndexOf('[');
            var id = keyFunc((index < 0) ? itemExpr : itemExpr.Substring(0, index));
            var num = (index < 0) ? defaultValue : valueFunc(itemExpr.TrimParentheses());
            items.Add(new IKeyValuePair<TKey, TValue>(id, num));
        }
        return items;
    }
}

public class Linear<T> : IKeyValuePair<T, T>
{
    public T mult
    {
        get => key;
        set => key = value;
    }

    public T add
    {
        get => value;
        set => this.value = value;
    }

    public Linear() : base() { }
    public Linear(T k, T v) : base(k, v) { }
    public Linear(Linear<T> rhs) : base(rhs) { }
    
    public static List<Linear<T>> Parse(string text, Func<string, T> parseFunc, T defaultValue = default(T), string delimeter = "/")
    {
        var items = Parse(text, parseFunc, parseFunc, defaultValue, delimeter);
        return items?.Select(x => new Linear<T>(x.key, x.value)).ToList();
    }
}

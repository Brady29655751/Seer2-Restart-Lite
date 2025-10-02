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
    
}

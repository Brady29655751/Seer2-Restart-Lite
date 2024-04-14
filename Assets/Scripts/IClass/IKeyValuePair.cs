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

    public IKeyValuePair() {}
    public IKeyValuePair(TKey k, TValue v) {
        key = k;
        value = v;
    }
}


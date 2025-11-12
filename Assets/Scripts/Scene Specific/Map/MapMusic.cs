using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapMusic
{
    [XmlAttribute("category")] public int category;
    [XmlElement("bgm")] public string bgm;
    [XmlElement("fx")] public string fx;

    public bool ValueEquals(MapMusic other) {
        if (other == null)
            return false;
            
        return category == other.category && bgm == other.bgm && fx == other.fx;
    }
}

public class MapAnim
{
    [XmlAttribute("pos")] public string pos;
    [XmlAttribute("scale")] public string scale;

    public Vector3 animPos => string.IsNullOrEmpty(pos) ? Vector3.zero : new Vector3(pos.ToVector2().x, pos.ToVector2().y, 10);
    public Vector3 animScale => string.IsNullOrEmpty(scale) ? Vector3.zero : new Vector3(scale.ToVector2().x, scale.ToVector2().y, 1);
}
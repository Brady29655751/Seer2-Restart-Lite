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

public class AnimInfo
{
    [XmlAttribute("id")] public string id;
    [XmlAttribute("pos")] public string pos;
    [XmlAttribute("scale")] public string scale;
    [XmlAttribute("rotation")] public string npcRotation = "0,0,0";

    public Vector3 animPos => string.IsNullOrEmpty(pos) ? new Vector3(0, 0, 1) : new Vector3(pos.ToVector2().x, pos.ToVector2().y, 1);
    public Vector3 animScale => string.IsNullOrEmpty(scale) ? Vector3.one : new Vector3(scale.ToVector2().x, scale.ToVector2().y, 1);
    public Quaternion animRot => npcRotation.ToQuaternion();
}
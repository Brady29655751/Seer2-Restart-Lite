using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportInfo
{
    [XmlAttribute("id")] public int id;
    [XmlAttribute("name")] public string name;
    [XmlAttribute("pos")] public string posId;
    public Vector2 pos => posId.ToVector2();
    [XmlAttribute("targetMapId")] public int targetMapId;

}

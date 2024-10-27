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
    [XmlAttribute("color")] public string rgba = "255,255,255,128";
    public Color color => rgba.ToColor(Color.white);

    [XmlAttribute("targetMapId")] public int targetMapId;

}

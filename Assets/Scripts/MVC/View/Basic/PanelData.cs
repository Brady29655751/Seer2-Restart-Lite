using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("panel")]
public class PanelData
{
    [XmlAttribute("name")] public string name;
    [XmlAttribute("size")] public string panelSize;
    public Vector2 size => panelSize.ToVector2();
    
    [XmlElement("npc")] public List<NpcInfo> npcs;
    
}

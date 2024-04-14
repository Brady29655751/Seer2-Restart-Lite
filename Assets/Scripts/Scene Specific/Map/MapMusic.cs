using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMusic
{
    [XmlAttribute("category")] public int category;
    [XmlElement("bgm")] public string bgm;
    [XmlElement("fx")] public string fx;
}
using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RM = ResourceManager;
using System.Threading.Tasks;

public class DialogInfo
{
    [XmlAttribute] public string id;
    [XmlAttribute("icon")] public string iconId;
    public Sprite icon => GetIcon(iconId);
    
    [XmlAttribute("size")] public string iconSize;
    public Vector2 size => iconSize.ToVector2();
    [XmlAttribute("pos")] public string iconPos;
    public Vector2 pos => iconPos.ToVector2();
    [XmlAttribute("color")] public string iconColor = "255,255,255,255";
    public Color color => iconColor.ToColor();
    

    [XmlAttribute] public string name;
    [XmlElement] public string content;

    [XmlArray("functionHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "func")] 
    public List<NpcButtonHandler> functionHandler;

    [XmlArray("replyHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "reply")] 
    public List<NpcButtonHandler> replyHandler;

    public static Sprite GetIcon(string resId) {
        return NpcInfo.GetIcon(resId);
    }

}

using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RM = ResourceManager;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class NpcInfo
{
    [XmlAttribute] public int id;
    [XmlAttribute] public string resId;
    public Sprite icon => GetIcon(resId);

    [XmlAttribute] public string name;
    [XmlElement] public string description;
    [XmlAttribute] public bool active = true;
    [XmlAttribute("target")] public bool raycastTarget = true;

    [XmlAttribute("color")] public string rgba = "255,255,255,255";
    public Color color => rgba.ToColor(Color.white);
    [XmlAttribute("size")] public string npcSize;
    public Vector2 size => npcSize.ToVector2();
    [XmlAttribute("pos")] public string npcPos;
    public Vector2 pos => npcPos.ToVector2();
    [XmlAttribute("rotation")] public string npcRotation = "0,0,0";
    public Quaternion rotation => npcRotation.ToQuaternion();
    [XmlAttribute("namePos")] public string npcNamePos = "0,5";
    public Vector2 namePos => npcNamePos.ToVector2();

    [XmlElement("transport")] public string transport;
    public Vector2 transportPos => transport.ToVector2(pos);

    [XmlArray("dialog"), XmlArrayItem(typeof(DialogInfo), ElementName = "branch")] 
    public List<DialogInfo> dialogHandler;

    [XmlArray("battle"), XmlArrayItem(typeof(BattleInfo), ElementName = "branch")] 
    public List<BattleInfo> battleHandler;

    [XmlArray("eventHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> eventHandler;

    public static Sprite GetIcon(string resId) {
        if (int.TryParse(resId, out _))
            return ResourceManager.instance.GetLocalAddressables<Sprite>("Npc/" + resId);
        
        return ResourceManager.instance.GetLocalAddressables<Sprite>(resId);
    }
}

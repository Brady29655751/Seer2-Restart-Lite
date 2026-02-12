using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RM = ResourceManager;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Linq;

public class NpcInfo
{
    [XmlAttribute] public int id;
    [XmlAttribute] public string resId;
    public Sprite icon => GetIcon(resId);
    public GameObject anim => GetAnim(animInfo?.id);
    [XmlElement("anim")] public AnimInfo animInfo;

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
    [XmlAttribute("nameSize")] public int nameSize = 0;
    [XmlAttribute("nameColor")] public string nameRgba = "0,64,255,255";
    public Color nameColor => nameRgba.ToColor(new Color32(0, 64, 255, 255));
    [XmlAttribute("nameFont")] public string nameFont;


    [XmlElement("transport")] public string transport;
    public Vector2 transportPos => transport.ToVector2(pos);

    [XmlArray("dialog"), XmlArrayItem(typeof(DialogInfo), ElementName = "branch")] 
    public List<DialogInfo> dialogHandler;

    [XmlArray("battle"), XmlArrayItem(typeof(BattleInfo), ElementName = "branch")] 
    public List<BattleInfo> battleHandler;

    [XmlArray("eventHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> eventHandler;

    [XmlArray("callbackHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")]
    public List<NpcButtonHandler> callbackHandler;

    public static bool IsMod(string resId) {
        if (int.TryParse(resId, out var modId))
            return modId <= -50001_00;

        return resId.StartsWith("Mod/");
    }

    public static Sprite GetIcon(string resId) {
        if (string.IsNullOrEmpty(resId) || (resId == "0") || (resId == "null") || (resId == "none")) 
            return SpriteSet.Empty;
            
        if (int.TryParse(resId, out _))
            return ResourceManager.instance.GetLocalAddressables<Sprite>("Npc/" + resId, IsMod(resId));
        
        bool isMod = IsMod(resId);
        var iconId = isMod ? resId.TrimStart("Mod/") : resId;
        return ResourceManager.instance.GetLocalAddressables<Sprite>(iconId, isMod);
    }

    public static GameObject GetAnim(string animId)
    {
        if (string.IsNullOrEmpty(animId) || (animId == "0") || (animId == "null") || (animId == "none")) 
            return null;

        var petIdExpr = animId.TrimStart("Pets/anim/");
        var split = petIdExpr.Split('-');
        return ResourceManager.instance.GetPetAnimPrefab(int.Parse(split[0]), split.Length == 1 ? petIdExpr + "-idle" : petIdExpr);
    }
}

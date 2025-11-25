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
    [XmlElement("content")] public string rawContent;
    public string content => GetContent();

    [XmlArray("functionHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "func")] 
    public List<NpcButtonHandler> functionHandler;

    [XmlArray("replyHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "reply")] 
    public List<NpcButtonHandler> replyHandler;

    public static Sprite GetIcon(string resId) {
        return NpcInfo.GetIcon(resId);
    }

    public string GetContent()
    {
        var result = rawContent;
        int startIndex = result.IndexOf("{expr:");
        if (startIndex < 0)
            return result;

        int endIndex = result.IndexOf("}", startIndex);
        
        while ((startIndex >= 0) && (endIndex > startIndex))
        {
            string expr = result.Substring(startIndex + 6, endIndex - startIndex - 6);
            string evalResult = Identifier.GetIdentifier(expr).ToString();

            result = result.Substring(0, startIndex) + evalResult + result.Substring(endIndex + 1);

            startIndex = result.IndexOf("{expr:");
            if (startIndex < 0)
                break;

            endIndex = result.IndexOf("}", startIndex);
        }

        return result;
    }

}

using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class PetKizunaInfo
{
    [XmlAttribute("id")] public int id;

    [XmlArray("tagList"), XmlArrayItem(typeof(string), ElementName = "tag")]
    public List<string> tagList;

    [XmlArray("dialog"), XmlArrayItem(typeof(PetKizunaDialog), ElementName = "branch")]
    public List<PetKizunaDialog> dialogs = new List<PetKizunaDialog>();

    public PetKizunaInfo(){}

    public List<string> GetDialogCandidates(int kizuna)
    {
        return dialogs.Where(x => x.Condition(kizuna)).OrderByDescending(x => x.kizuna).FirstOrDefault()?.contents;
    }
}

public class PetKizunaDialog
{
    [XmlAttribute("kizuna")] public int kizuna;
    [XmlElement("content")] public List<string> contents = new List<string>();
    [XmlElement("condition")] public List<string> condition;

    public Func<bool> GetDialogCondition()
    {
        var handler = new NpcButtonHandler(){ condition = this.condition };
        return NpcHandler.GetNpcCondition(null, handler, null);
    }

    public bool Condition(int kizuna) 
    {
        return (kizuna >= this.kizuna) && GetDialogCondition().Invoke();
    }
}
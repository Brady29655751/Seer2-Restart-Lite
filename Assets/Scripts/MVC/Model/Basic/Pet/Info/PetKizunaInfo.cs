using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using ExitGames.Client.Photon.StructWrapping;

public class PetKizunaInfo
{
    [XmlAttribute("id")] public int id;

    [XmlArray("tagList"), XmlArrayItem(typeof(string), ElementName = "tag")]
    public List<string> tagList;

    [XmlArray("dialog"), XmlArrayItem(typeof(PetKizunaDialog), ElementName = "branch")]
    public List<PetKizunaDialog> dialogs = new List<PetKizunaDialog>();

    [XmlArray("gift"), XmlArrayItem(typeof(PetKizunaGift), ElementName = "branch")]
    public List<PetKizunaGift> gifts = new List<PetKizunaGift>();

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

    public Func<bool> GetCondition()
    {
        var handler = new NpcButtonHandler(){ condition = this.condition };
        return NpcHandler.GetNpcCondition(null, handler, null);
    }

    public bool Condition(int kizuna) 
    {
        return (kizuna >= this.kizuna) && GetCondition().Invoke();
    }
}

public class PetKizunaGift
{
    [XmlAttribute("kizuna")] public int kizuna;
    [XmlElement("item")] public List<string> items = new List<string>();
    [XmlIgnore] public List<Item> Gifts => items.Select(x => new Item(x.ToIntList().Get(0), x.ToIntList().Get(1))).ToList();   
    [XmlIgnore] public string RecordKey => $"kizunaGift[{this.kizuna}]";

    public bool Condition(Pet pet) 
    {
        if (pet == null)
            return false;

        if (Gifts.Exists(x => (x == null) || (Item.GetItemInfo(x.id) == null)))
            return false;

        return (pet.kizuna >= this.kizuna) && (!pet.record.GetRecord(RecordKey, false));
    }

    public void Apply(Pet pet)
    {
        if (pet == null)
            return;

        Gifts?.ForEach(x => { Item.Add(x); Item.OpenHintbox(x); });
        pet.record.SetRecord(RecordKey, true);
    }

    public bool CheckAndApply(Pet pet)
    {
        if (!Condition(pet))
            return false;

        Apply(pet);
        return true;
    }
}
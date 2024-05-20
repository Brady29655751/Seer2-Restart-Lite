using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeature
{
    [XmlAttribute] public int id;
    public bool hasEmblem = true;   // 是否配戴紋章
    public List<int> afterwardBuffIds;
    [XmlIgnore] public List<Buff> afterwardBuffs => afterwardBuffIds?.Select(x => new Buff(x)).ToList();

    [XmlIgnore] public PetFeatureInfo info => Database.instance.GetPetInfo(id)?.feature;

    public PetFeature() {
        afterwardBuffIds = new List<int>();
    }
    public PetFeature(int _id, bool _emblem = true) {
        id = _id;
        hasEmblem = _emblem;
        afterwardBuffIds = new List<int>();
    }

    public PetFeature(PetFeature rhs) {
        id = rhs.id;
        hasEmblem = rhs.hasEmblem;
        afterwardBuffIds = new List<int>(rhs.afterwardBuffIds);
    }
}

using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeature
{
    [XmlAttribute] public int id;
    public bool hasEmblem = true;   // 是否配戴紋章

    [XmlIgnore] public PetFeatureInfo info => Database.instance.GetPetInfo(id)?.feature;

    public PetFeature() {}
    public PetFeature(int _id, bool _emblem = true) {
        id = _id;
        hasEmblem = _emblem;
    }

    public PetFeature(PetFeature rhs) {
        id = rhs.id;
        hasEmblem = rhs.hasEmblem;
    }
}

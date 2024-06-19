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
    public int featureId = 0, emblemId = 0;
    public List<int> afterwardBuffIds;

    [XmlIgnore] public Feature feature => Database.instance.GetFeatureInfo((featureId == 0) ? defaultInfo.baseId : featureId)?.feature;
    [XmlIgnore] public Emblem emblem => Database.instance.GetFeatureInfo((emblemId == 0) ? defaultInfo.baseId : emblemId)?.emblem;
    [XmlIgnore] public List<Buff> afterwardBuffs => afterwardBuffIds?.Select(x => new Buff(x)).ToList();

    [XmlIgnore] public PetFeatureInfo info => new PetFeatureInfo(id, feature, emblem);
    [XmlIgnore] public PetFeatureInfo defaultInfo => Database.instance.GetPetInfo(id)?.feature;

    public static PetFeatureInfo GetFeatureInfo(int id) => Database.instance.GetFeatureInfo(id);

    public PetFeature() {
        afterwardBuffIds = new List<int>();
    }
    public PetFeature(int _id, bool _emblem = true, int _featureId = 0, int _emblemId = 0) {
        id = _id;
        hasEmblem = _emblem;
        featureId = _featureId;
        emblemId = _emblemId;
        afterwardBuffIds = new List<int>();
    }

    public PetFeature(PetFeature rhs) {
        id = rhs.id;
        hasEmblem = rhs.hasEmblem;
        featureId = rhs.featureId;
        emblemId = rhs.emblemId;
        afterwardBuffIds = new List<int>(rhs.afterwardBuffIds);
    }
}

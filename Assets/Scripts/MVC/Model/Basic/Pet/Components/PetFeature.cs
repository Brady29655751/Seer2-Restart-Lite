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
    public PetFeature(int _id, bool _emblem = true, int _featureId = 0, int _emblemId = 0, List<int> _afterwardBuffIds = null) {
        id = _id;
        hasEmblem = _emblem;
        featureId = _featureId;
        emblemId = _emblemId;
        afterwardBuffIds = _afterwardBuffIds ?? new List<int>();
    }

    public PetFeature(PetFeature rhs) {
        id = rhs.id;
        hasEmblem = rhs.hasEmblem;
        featureId = rhs.featureId;
        emblemId = rhs.emblemId;
        afterwardBuffIds = new List<int>(rhs.afterwardBuffIds);
    }

    public Status GetExtraStatus(Status normalStatus) {
        var buffs = afterwardBuffIds?.Select(Buff.GetBuffInfo).Where(x => x != null);
        var mult = buffs?.Select(x => new Status(x.options.Get("status_mult", "1/1/1/1/1/1").ToFloatList('/')))
            .Aggregate(Status.one, (sum, next) => sum * next) ?? Status.one;
        var add = buffs?.Select(x => new Status(x.options.Get("status", "0/0/0/0/0/0").ToFloatList('/')))
            .Aggregate(Status.zero, (sum, next) => sum + next) ?? Status.zero;
        return Status.FloorToInt((mult - Status.one) * normalStatus + add);
    }

    public int SetTrait(int traitId = 0) {
        if (traitId == -1)
            return traitId;

        afterwardBuffIds.RemoveAll(x => Buff.GetBuffInfo(x)?.options.Get("group") == "trait");
        if (traitId == 0)
            traitId = BuffInfo.database.Where(x => x.options.Get("group") == "trait").Select(x => x.id).ToList().Random();
        
        afterwardBuffIds.Add(traitId);
        return traitId;
    }
}

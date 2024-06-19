using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class PetFeatureInfo 
{
    public const int DATA_COL = 5;

    public int baseId;
    public Feature feature;
    public Emblem emblem;

    public PetFeatureInfo(){}
    public PetFeatureInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        baseId = int.Parse(_slicedData[0]);
        feature = new Feature() { baseId = baseId, name = _slicedData[1], description = _slicedData[2] };
        emblem = (_slicedData[3] == "none") ? null : new Emblem() { baseId = baseId, name = _slicedData[3], description = _slicedData[4] };
    }

    public PetFeatureInfo(int baseId, Feature feature, Emblem emblem) {
        this.baseId = baseId;
        this.feature = feature;
        this.emblem = emblem;
    }

    public string[] GetRawInfoStringArray() {
        return new string[] { baseId.ToString(), feature.name, feature.description, emblem.name, emblem.description };
    }

}

[XmlRoot("feature")]
public class Feature {
    public int baseId;
    public string name;
    public string description;
}

[XmlRoot("emblem")]
public class Emblem {
    public int baseId;
    public string name;
    public string description;

    public static Sprite GetNullEmblemSprite() {
        return GetSprite(0);
    }

    public static Sprite GetSprite(int baseId) {
        return PetUISystem.GetEmblemIcon(baseId);
    }

    public Sprite GetSprite() {
        return GetSprite(baseId);
    }
    
}
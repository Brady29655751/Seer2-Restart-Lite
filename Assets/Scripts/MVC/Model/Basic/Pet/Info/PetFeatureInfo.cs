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
        emblem = new Emblem() { baseId = baseId, name = _slicedData[3], description = _slicedData[4] };
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
        return ResourceManager.instance.GetSprite("Emblems/0");
    }

    public static Sprite GetSprite(int baseId) {
        return PetUISystem.GetEmblemIcon(baseId);
    }

    public Sprite GetSprite() {
        return GetSprite(baseId);
    }
    
}
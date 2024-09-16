using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSoundInfo
{
    public const int DATA_COL = 7;

    public int skinId;
    public string name;
    public string physics, attribute, special, critical, fit;

    public PetSoundInfo()
    {
    }

    public PetSoundInfo(string[] _data, int startIndex = 0)
    {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        skinId = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        physics = _slicedData[2];
        attribute = _slicedData[3];
        special = _slicedData[4];
        critical = _slicedData[5];
        fit = _slicedData[6].Trim();
    }

    public PetSoundInfo(int skinId, string name, string physics, string attribute, string special, string critical, string fit)
    {
        this.skinId = skinId;
        this.name = name;
        this.physics = physics;
        this.attribute = attribute;
        this.special = special;
        this.critical = critical;
        this.fit = fit;
    }

    public string GetSoundByType(PetAnimationType type)
    {
        return type switch
        {
            PetAnimationType.Physic => physics,
            PetAnimationType.Property => attribute,
            PetAnimationType.Special => special,
            PetAnimationType.Super => critical,
            PetAnimationType.SecondSuper => fit,
            _ => "0"
        };
    }

    public string[] GetRawInfoStringArray()
    {
        return new string[]
        {
            skinId.ToString(), name, physics, attribute, special, critical, fit
        };
    }

}

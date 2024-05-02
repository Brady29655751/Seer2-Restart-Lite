using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetExpInfo
{
    public const int DATA_COL = 5;

    public int id;
    public int expType;
    public int evolvePetId;
    public int evolveLevel;
    public int beatExpParam;  // 被擊敗時產出經驗之係數

    public PetExpInfo(){}
    public PetExpInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        expType = int.Parse(_slicedData[1]);
        evolvePetId = int.Parse(_slicedData[2]);
        evolveLevel = int.Parse(_slicedData[3]);
        beatExpParam= int.Parse(_slicedData[4]);
    }

    public PetExpInfo(int id, int expType, int evolvePetId, int evolveLevel, int beatExpParam) {
        this.id = id;
        this.expType = expType;
        this.evolvePetId = evolvePetId;
        this.evolveLevel = evolveLevel;
        this.beatExpParam = beatExpParam;
    }

    public string[] GetRawInfoStringArray() {
        return new string[] { id.ToString(), expType.ToString(), 
            evolvePetId.ToString(), evolveLevel.ToString(), beatExpParam.ToString() };
    }
}

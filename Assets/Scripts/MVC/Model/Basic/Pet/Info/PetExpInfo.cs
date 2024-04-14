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
}

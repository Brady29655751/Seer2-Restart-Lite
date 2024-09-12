using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetExpInfo
{
    public const int DATA_COL = 5;

    public int id;
    public int expType;
    public List<int> evolvePetIds;
    public List<int> evolveLevels;
    public int evolvePetId => evolvePetIds.FirstOrDefault();
    public int evolveLevel => evolveLevels.FirstOrDefault();
    public int beatExpParam;  // 被擊敗時產出經驗之係數

    public PetExpInfo(){}
    public PetExpInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        expType = int.Parse(_slicedData[1]);
        evolvePetIds = _slicedData[2].ToIntList('/');
        evolveLevels = _slicedData[3].ToIntList('/');
        beatExpParam= int.Parse(_slicedData[4]);
    }

    public PetExpInfo(int id, int expType, List<int> evolvePetIds, List<int> evolveLevels, int beatExpParam) {
        this.id = id;
        this.expType = expType;
        this.evolvePetIds = evolvePetIds;
        this.evolveLevels = evolveLevels;
        this.beatExpParam = beatExpParam;
    }

    public string[] GetRawInfoStringArray() {
        return new string[] { id.ToString(), expType.ToString(), 
            evolvePetIds.Select(x => x.ToString()).ConcatToString("/"), 
            evolveLevels.Select(x => x.ToString()).ConcatToString("/"),
            beatExpParam.ToString() };
    }
}

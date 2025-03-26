using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PetBasicInfo
{
    public const int DATA_COL = 10;

    public int id;
    public int baseId;
    public string name;
    public int elementId, subElementId;
    public Element element => (Element)elementId;
    public Element subElement => (Element)subElementId;
    public Status baseStatus = new Status();

    public int gender => genderList.FirstOrDefault();     // 0 for male, 1 for female, 2 for both, -1 for none.
    public string rawGender;
    public List<int> genderList = new List<int>();
    public List<int> genderDistribution = new List<int>();
    public int baseHeight, baseWeight;
    public string description;
    public string habitat;
    public string linkId;

    public PetBasicInfo(){}
    public PetBasicInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        baseId = int.Parse(_slicedData[1]);
        name = _slicedData[2];

        var elementList = _slicedData[3].ToIntList('/');
        elementId = elementList[0];
        subElementId = (elementList.Count > 1) ? elementList[1] : 0;

        baseStatus = new Status(_slicedData[4].ToFloatList('/'));
        rawGender = _slicedData[5];
        TryParseGender(_slicedData[5], out genderList, out genderDistribution);
        baseHeight = _slicedData[6].ToIntList('/')[0];
        baseWeight = _slicedData[6].ToIntList('/')[1];
        description = _slicedData[7];
        habitat = _slicedData[8];
        linkId = _slicedData[9].TrimEnd();
    }

    public PetBasicInfo(int id, int baseId, string name, Element element, Element subElement, Status baseStatus,
        string gender, int baseHeight, int baseWeight, string description, string habitat, string linkId) {
        this.id = id;
        this.baseId = baseId;
        this.name = name.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.elementId = (int)element;
        this.subElementId = (int)subElement;
        this.baseStatus = new Status(baseStatus);
        this.rawGender = gender;
        TryParseGender(gender, out genderList, out genderDistribution);
        this.baseHeight = baseHeight;
        this.baseWeight = baseWeight;
        this.description = description.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.habitat = habitat.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.linkId = linkId.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
    }

    public string[] GetRawInfoStringArray() {
        return new string[] { id.ToString(), baseId.ToString(), name, elementId.ToString() + ((subElementId == 0) ? string.Empty : ("/" + subElementId)),
            baseStatus.ToString("/"), gender.ToString(), baseHeight + "/" + baseWeight,
            description, habitat, linkId
        };
    }

    public static bool TryParseGender(string genderExpr, out List<int> genderList, out List<int> genderDistribution) {
        genderList = new List<int>();
        genderDistribution = new List<int>();

        if (int.TryParse(genderExpr, out var uniqueGender)) {
            genderList.Add(uniqueGender);
            genderDistribution = new List<int>(){ 1 };
            return true;
        }

        var genderInfo = genderExpr.Split('/');
        for (int i = 0; i < genderInfo.Length; i++) {
            if (!int.TryParse(genderInfo[i].TrimParentheses(), out int p))
                return false;

            if (!int.TryParse(genderInfo[i].TrimEnd("[" + p + "]"), out int g))
                return false;

            genderList.Add(g);
            genderDistribution.Add(p);
        }
        
        return true;
    }
}

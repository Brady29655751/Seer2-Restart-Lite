using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int gender;     // 0 for male, 1 for female, 2 for both, -1 for special.
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
        gender = int.Parse(_slicedData[5]);
        baseHeight = _slicedData[6].ToIntList('/')[0];
        baseWeight = _slicedData[6].ToIntList('/')[1];
        description = _slicedData[7];
        habitat = _slicedData[8];
        linkId = _slicedData[9].TrimEnd();
    }

    public PetBasicInfo(int id, int baseId, string name, Element element, Element subElement, Status baseStatus,
        int gender, int baseHeight, int baseWeight, string description, string habitat, string linkId) {
        this.id = id;
        this.baseId = baseId;
        this.name = name.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.elementId = (int)element;
        this.subElementId = (int)subElement;
        this.baseStatus = new Status(baseStatus);
        this.gender = gender;
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
}

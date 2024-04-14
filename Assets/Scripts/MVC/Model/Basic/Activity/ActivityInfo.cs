using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RM = ResourceManager;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class ActivityInfo 
{
    public const int DATA_COL = 10;
    public string id;
    public string resId;
    public string name;
    public ActivityType type;
    public string time;
    public string linkId;
    public string description;
    public List<string> rewardIcons;
    public DateTime releaseDate;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public Sprite activityBackground => GetBackground(resId);

    public ActivityInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = _slicedData[0];
        resId = _slicedData[1];
        name = _slicedData[2];
        type = _slicedData[3].ToActivityType();
        time = _slicedData[4];
        linkId = _slicedData[5];
        rewardIcons = _slicedData[6].Split('&').ToList();
        if (rewardIcons.Count < 3)
            rewardIcons.AddRange(Enumerable.Repeat("none", 3 - rewardIcons.Count));

        releaseDate = DateTime.Parse(_slicedData[7]);
        options.ParseOptions(_slicedData[8]);
        description = _slicedData[9];
    }

    public static Sprite GetBackground(string resId) {
        if (int.TryParse(resId, out _))
            return ResourceManager.instance.GetLocalAddressables<Sprite>("Activities/" + resId);
        else
            return ResourceManager.instance.GetLocalAddressables<Sprite>(resId);
    }

}

public static class ActivityDatabase {
    public static Dictionary<string, ActivityType> typeConvDict = new Dictionary<string, ActivityType>() {
        {"none", ActivityType.None},
        {"resident", ActivityType.Resident},
        {"daily", ActivityType.Daily},
    };

    public static ActivityType ToActivityType(this string type) {
        return typeConvDict.Get(type, ActivityType.None);
    }
}

public enum ActivityType {
    None = 0,
    Resident = 1,
    Daily = 2,
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuffInfo
{
    public const int DATA_COL = 7;
    public static List<BuffInfo> database => Database.instance.buffInfoDict.Select(x => x.Value).Where(x => x != null)
        .OrderByDescending(x => x.id.GetSortPriority()).ToList();

    /* Basic Data */
    public int id { get; private set; }
    public int resId { get; private set; }
    public int itemId { get; private set; }
    public string name { get; private set; }
    public BuffType type { get; private set; }
    public CopyHandleType copyHandleType { get; private set; }
    public int turn { get; private set; }
    public bool keep { get; private set; }  // 換場保留
    public bool inherit { get; private set; }   // 換場繼承
    public bool legacy { get; private set; }   // 死亡繼承
    public bool hide { get; private set; }  // 不予顯示
    public bool autoRemove { get; private set; }    // value為0以下時自動移除
    public bool movable { get; private set; }    // 可否行動 (異常類判斷)
    public string description { get; private set; }
    public Dictionary<string, string> options { get; private set; } = new Dictionary<string, string>();
    
    public int minValue = int.MinValue, maxValue = int.MaxValue;
    public int sortPriority => GetSortPriority();
    public string position => options.Get("position", "none");
    public List<Effect> effects { get; private set; }
    public Sprite icon => GetIcon();

    public static bool IsMod(int id) {
        return id.IsWithin(51_0001, 59_9999) || id.IsWithin(80_0013, 89_9999) || id.IsWithin(90_0013, 99_9999) || (id < -10_0000);
    }

    public BuffInfo(int id, string name, BuffType type, CopyHandleType copyHandleType, int turn, string options, string description) {
        this.id = id;
        this.name = name.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.type = type;
        this.copyHandleType = copyHandleType;
        this.turn = turn;
        this.options.ParseOptions(options.ReplaceSpecialWhiteSpaceCharacters(string.Empty));
        this.description = description.ReplaceSpecialWhiteSpaceCharacters(string.Empty);;

        InitOptionsProperty();
    }

    public BuffInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        type = _slicedData[2].ToBuffType();
        copyHandleType = _slicedData[3].ToCopyHandleType();
        turn = int.Parse(_slicedData[4]);
        options.ParseOptions(_slicedData[5]);
        description = _slicedData[6].Trim();

        InitOptionsProperty();
    }

    private void InitOptionsProperty() {
        resId = int.Parse(options.Get("res", id.ToString()));
        itemId = int.Parse(options.Get("item", "0"));
        keep = GetKeepInfo();
        inherit = ((type == BuffType.Feature) || (type == BuffType.Emblem)) ? false : bool.Parse(options.Get("inherit", "false"));
        legacy = bool.Parse(options.Get("legacy", "false"));
        hide = bool.Parse(options.Get("hide", "false"));
        autoRemove = bool.Parse(options.Get("auto_remove", "false"));
        minValue = int.Parse(options.Get("min_val", int.MinValue.ToString()));
        maxValue = int.Parse(options.Get("max_val", int.MaxValue.ToString()));
        movable = (type != BuffType.Abnormal) || (bool.Parse(options.Get("movable", "false")));
    }

    private bool GetKeepInfo() {
        if ((type == BuffType.Unhealthy) || (type == BuffType.Abnormal))
            return false;

        if ((type == BuffType.Feature) || (type == BuffType.Emblem))
            return true;

        return bool.Parse(options.Get("keep", "true"));
    }

    public void SetEffects(List<Effect> _effects) {
        effects = _effects;
    }

    public string[] GetRawInfoStringArray() {
        string[] defaultOptionKeys = new string[] { "res", "hide", "keep", "inherit", "legacy",
            "auto_remove", "min_val", "max_val" };

        string resRawString = (id == resId) ? string.Empty : ("res=" + resId + "&");
        string hideString = hide ? ("hide=true&") : string.Empty;
        string legacyString = legacy ? ("legacy=true&") : string.Empty;
        string autoRemoveString = autoRemove ? ("auto_remove=true&") : string.Empty;
        string minValueString = (minValue == int.MinValue) ? string.Empty : ("min_val=" + minValue + "&");
        string maxValueString = (maxValue == int.MaxValue) ? string.Empty : ("max_val=" + maxValue + "&");

        string rawOptionString = resRawString + minValueString + maxValueString + 
            hideString + legacyString + autoRemoveString +
            "keep=" + keep + "&inherit=" + inherit + "&";
        string otherOptionString = options.Where(entry => !defaultOptionKeys.Contains(entry.Key)).Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");
        string allOptionString = rawOptionString + otherOptionString; 

        return new string[] { id.ToString(), name, type.ToRawString(), copyHandleType.ToRawString(),
            turn.ToString(), allOptionString.TrimEnd("&"), description };
    }

    public int GetSortPriority() {
        int mod = 10_0000;
        int _type = id / mod;
        int _pet = id % mod;

        if ((id == -5) || (id == -6))
            return int.MaxValue;

        if (type == BuffType.Feature) {
            return (-2 * mod) + (_pet - mod);
        }
        if (type == BuffType.Emblem) {
            return (-1 * mod) + (_pet - mod);
        }
        return ((id < 0) ? 10 : 0) * mod + id;
    }

    public Sprite GetIcon() {
        int _type = id / 10_0000;
        int _pet = id % 10_0000;

        var sprite = ResourceManager.instance.GetLocalAddressables<Sprite>("Buffs/" + resId, IsMod(resId));
        
        if (type == BuffType.Feature)
            return sprite ?? PetUISystem.GetPetIcon(_pet * (_type < 5 ? 1 : -1));

        if (type == BuffType.Emblem)
            return sprite ?? PetUISystem.GetEmblemIcon(_pet * (_type < 5 ? 1 : -1));

        return sprite;
    }

}


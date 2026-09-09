using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetTransformInfo
{
    public const int DATA_COL = 7;

    public int skinId;
    public string name;
    public Dictionary<PetAnimationType, Vector2> posDict = new Dictionary<PetAnimationType, Vector2>();
    public Dictionary<PetAnimationType, Vector2> scaleDict = new Dictionary<PetAnimationType, Vector2>();

    // Idle[pos:2/3][scale:5/6], Super[pos:4/5][scale:6/7]
    public PetTransformInfo()
    {
    }

    public PetTransformInfo(string[] _data)
    {
        skinId = int.Parse(_data[0]);
        name = _data[1];

        for (int i = 2; i < _data.Length; i++)
        {
            var lp = _data[i].IndexOf('[');
            if (lp == -1)
                continue;

            var typeExpr = _data[i].Substring(0, lp).Trim();
            var info = _data[i].Substring(lp).TrimParenthesesLoop()?.Select(x => 
            {
                var split = x.Split(':');
                return split.Get(1, split.FirstOrDefault());
            }).ToList();

            if (ListHelper.IsNullOrEmpty(info))
                continue;
            
            if (!Enum.TryParse(typeExpr, true, out PetAnimationType type))
            {
                posDict.Add(PetAnimationType.None, info[0].ToVector2(delimeter: '/'));
                if (info.Count > 1)
                    scaleDict.Add(PetAnimationType.None, info[1].ToVector2(defaultValue: Vector2.one, delimeter: '/'));
                
                break;
            }

            posDict.Add(type, info[0].ToVector2(delimeter: '/'));
            if (info.Count > 1)
                scaleDict.Add(type, info[1].ToVector2(defaultValue: Vector2.one, delimeter: '/'));
        }
    }

    public Vector2 GetPositionByType(PetAnimationType type, Vector2 defaultValue = default(Vector2))
    {
        if (posDict.TryGetValue(PetAnimationType.None, out Vector2 pos))
            return pos;

        return posDict.Get(type, defaultValue);
    }

    public Vector2 GetScaleByType(PetAnimationType type, Vector2 defaultValue = default(Vector2))
    {
        if (scaleDict.TryGetValue(PetAnimationType.None, out Vector2 scale))
            return scale;

        return scaleDict.Get(type, defaultValue);
    }

}

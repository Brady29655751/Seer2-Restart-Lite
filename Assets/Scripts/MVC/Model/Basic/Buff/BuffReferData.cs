using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffReferData
{
    public int id, turn;
    public int? value = null;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public int ID => id;
    public int Turn => turn;
    public int Value => value ?? 0;

    public BuffReferData(){}

    public static BuffReferData Parse(string str, int? defaultValue = null, int defaultTurn = -1)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var data = new BuffReferData();
        var buffValueList = str.TrimParenthesesLoop();
        bool isWithValue = !ListHelper.IsNullOrEmpty(buffValueList);

        if (isWithValue)
        {
            var valueExpr = buffValueList.FirstOrDefault(x => !x.Contains(":"));
            var turnExpr = buffValueList.Where(x => !x.Contains(":")).ElementAtOrDefault(1);

            data.id = int.Parse(str.Substring(0, str.IndexOf('[')));
            data.value = string.IsNullOrEmpty(valueExpr) ? defaultValue : int.Parse(valueExpr);
            data.turn = string.IsNullOrEmpty(turnExpr) ? defaultTurn : int.Parse(turnExpr);
        }
        else
        {
            data.id = int.Parse(str);
            data.value = defaultValue;
            data.turn = defaultTurn;
        }

        buffValueList?.RemoveAll(x => !x.Contains(":"));
        buffValueList?.ForEach(x => data.options.Set(x.Split(':')[0], x.Split(':')[1]));

        return data;
    }

    public static Dictionary<int, BuffReferData> ParseDict(string str, int? defaultValue = null, int defaultTurn = -1)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        return str.Split('/').Select(x => BuffReferData.Parse(x, defaultValue, defaultTurn)).ToDictionary(data => data.id, data => data);
    }

    public static Buff CreateBuff(BuffReferData data)
    {
        if ((data == null) || (data.id == 0))
            return null;
        
        var buff = new Buff(data.id, data.turn, data.value);
        buff.options.Merge(data.options);
        return buff;
    }
}

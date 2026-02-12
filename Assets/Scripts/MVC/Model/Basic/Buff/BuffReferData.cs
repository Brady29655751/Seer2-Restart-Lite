using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffReferData
{
    public int id, value;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public BuffReferData(){}

    public static BuffReferData Parse(string str, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var data = new BuffReferData();
        var buffValueList = str.TrimParenthesesLoop();
        bool isWithValue = !ListHelper.IsNullOrEmpty(buffValueList);

        data.id = int.Parse(isWithValue ? str.Substring(0, str.IndexOf('[')) : str);
        data.value = isWithValue ? int.Parse(buffValueList.FirstOrDefault(x => !x.Contains(":")) ?? defaultValue.ToString()) : defaultValue;

        buffValueList?.RemoveAll(x => !x.Contains(":"));
        buffValueList?.ForEach(x => data.options.Set(x.Split(':')[0], x.Split(':')[1]));

        return data;
    }

    public static Dictionary<int, BuffReferData> ParseDict(string str, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        return str.Split('/').Select(x => BuffReferData.Parse(x, defaultValue)).ToDictionary(data => data.id, data => data);
    }
}

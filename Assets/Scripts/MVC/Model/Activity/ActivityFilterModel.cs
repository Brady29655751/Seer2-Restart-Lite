using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityFilterModel : FilterModel<ActivityInfo>
{
    public override FilterType GetFilterType() {
        return FilterType.Name;
    }

    public override Func<ActivityInfo, bool> GetFilter() {
        if (!isWorking)
            return (x) => true;

        FilterType filterType = type;
        string input = inputString;

        return x => x.name.Contains(input);
    }
}

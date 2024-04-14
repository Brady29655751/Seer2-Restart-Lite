using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionListView : ScrollListView<Mission>
{
    protected override string GetTitle(Mission obj)
    {
        return obj.info.title;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorView : Module
{
    [SerializeField] private Text modeText, floorText;

    public void SetMode(string mode) {
        string modeChineseText = mode switch {
            "easy" => "简单",
            "hard" => "困难",
            _ => "特殊",
        };
        modeText?.SetText(modeChineseText + "模式");
    }

    public void SetFloor(string floor) {
        floorText?.SetText(floor);
    }
}

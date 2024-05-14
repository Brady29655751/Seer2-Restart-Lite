using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfoModel : Module
{
    public int weather { get; private set; }
    public int dayNightSwitch { get; private set; }

    public void SetWeather(int weather) {
        this.weather = weather;
    }

    public void SetDayNightSwitch(int dayNightSwitch) {
        this.dayNightSwitch = dayNightSwitch;
    }
}

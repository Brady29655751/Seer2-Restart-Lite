using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfoModel : Module
{
    public Weather weather { get; private set; }
    public int dayNightSwitch { get; private set; }

    public void SetWeather(Weather weather) {
        this.weather = weather;
    }

    public void SetDayNightSwitch(int dayNightSwitch) {
        this.dayNightSwitch = dayNightSwitch;
    }
}

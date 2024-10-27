using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsData
{
    public float BGMVolume, UIVolume;
    public float battleBGMVolume, battleSEVolume;  
    public float battleAnimSpeed;
    public bool shakeWhenBigDamage, flashWhenBigDamage;

    public SettingsData() {
        //TODO in setting panel add shake/flashWhenBigDamage option
        BGMVolume = UIVolume = 10;
        battleBGMVolume = battleSEVolume = 10;  
        battleAnimSpeed = 1;
        shakeWhenBigDamage = true;
        flashWhenBigDamage = true;
    }
}

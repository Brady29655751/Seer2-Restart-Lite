using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SettingsData
{
    public bool useRobotAsPlayer;
    public float BGMVolume, UIVolume;
    public float battleBGMVolume, battleSEVolume;  
    public float battleAnimSpeed;
    public bool shakeWhenBigDamage, flashWhenBigDamage;
    public bool autoHealAfterBattle;
    public int initMapId = -70;
    public int ruleId;
    public int comboDamageDisplayMode;
    public string wildNpcBubbleStyle;
    [XmlIgnore] public BattleRule rule => (BattleRule)ruleId;

    public SettingsData()
    {
        useRobotAsPlayer = true;
        BGMVolume = UIVolume = 10;
        battleBGMVolume = battleSEVolume = 10;
        battleAnimSpeed = 1;
        shakeWhenBigDamage = true;
        flashWhenBigDamage = true;
        autoHealAfterBattle = false;
        initMapId = -70;
        ruleId = 0;
        wildNpcBubbleStyle = "white";
    }
}

public enum ComboDamageDisplayMode
{
    AllComboOnly = 0,
    TotalOnly = 1,
    AllComboAndTotal = 2,
}

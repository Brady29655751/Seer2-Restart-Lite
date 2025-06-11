using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSettingModel : Module
{
    public SettingsData settingsData => Player.instance.gameData.settingsData;
    public float BGMVolume { get; private set; }
    public float UIVolume { get; private set; }
    public float battleBGMVolume { get; private set; }
    public float battleSEVolume { get; private set; }
    public bool flashWhenBigDamage { get; private set; }
    public bool shakeWhenBigDamage { get; private set; }

    public float battleAnimSpeed { get; private set; }
    public int initMapId { get; private set; }

    public void InitVolume()
    {
        BGMVolume = settingsData.BGMVolume;
        UIVolume = settingsData.UIVolume;
        battleBGMVolume = settingsData.battleBGMVolume;
        battleSEVolume = settingsData.battleSEVolume;
        flashWhenBigDamage = settingsData.flashWhenBigDamage;
        shakeWhenBigDamage = settingsData.shakeWhenBigDamage;
        battleAnimSpeed = settingsData.battleAnimSpeed;
        initMapId = settingsData.initMapId;
    }

    public void OnConfirmSettings()
    {
        settingsData.BGMVolume = BGMVolume;
        settingsData.UIVolume = UIVolume;
        settingsData.battleBGMVolume = battleBGMVolume;
        settingsData.battleSEVolume = battleSEVolume;
        settingsData.flashWhenBigDamage = flashWhenBigDamage;
        settingsData.shakeWhenBigDamage = shakeWhenBigDamage;
        settingsData.battleAnimSpeed = battleAnimSpeed;
        settingsData.initMapId = initMapId;
        SaveSystem.SaveData();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
    }
    public void SetUIVolume(float value)
    {
        UIVolume = value;
    }
    public void SetBattleBGMVolume(float value)
    {
        battleBGMVolume = value;
    }
    public void SetBattleSEVolume(float value)
    {
        battleSEVolume = value;
    }

    public void SetFlash(bool isOn)
    {
        flashWhenBigDamage = isOn;
    }

    public void SetShake(bool isOn)
    {
        shakeWhenBigDamage = isOn;
    }

    public void SetBattleAnimSpeed(float value)
    {
        battleAnimSpeed = value;
    }

    public void SetInitMapId(int value)
    {
        initMapId = value;
    }

}

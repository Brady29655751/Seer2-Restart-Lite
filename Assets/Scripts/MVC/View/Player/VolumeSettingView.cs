using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingView : Module
{
    [SerializeField] private Hintbox confirmSettingsHintbox;
    [SerializeField] private List<ISlider> sliderBlockViews;    
    [SerializeField] private Toggle flashToggle, shakeToggle;
    [SerializeField] private Text battleAnimSpeedText, initMapText;

    public void SetSliderVolume(SettingsData settingsData)
    {
        sliderBlockViews[(int)VolumeOrder.BGM]?.SetSliderValue(settingsData.BGMVolume);
        sliderBlockViews[(int)VolumeOrder.BattleBGM]?.SetSliderValue(settingsData.battleBGMVolume);
        sliderBlockViews[(int)VolumeOrder.BattleSE]?.SetSliderValue(settingsData.battleSEVolume);
        sliderBlockViews[(int)VolumeOrder.UI]?.SetSliderValue(settingsData.UIVolume);
        SetFlashToggle(settingsData.flashWhenBigDamage);
        SetShakeToggle(settingsData.shakeWhenBigDamage);
        SetBattleAnimSpeedText(settingsData.battleAnimSpeed);
        SetInitMapText(settingsData.initMapId);
    }

    public void OnConfirmSettings() {
        confirmSettingsHintbox.SetHintboxActive(true);
    }

    public void SetBGMVolume(float value) {
        sliderBlockViews[(int)VolumeOrder.BGM]?.OnValueChanged(value);
    }

    public void SetBattleBGMVolume(float value) {
        sliderBlockViews[(int)VolumeOrder.BattleBGM]?.OnValueChanged(value);
    }
    public void SetBattleSEVolume(float value) {
        sliderBlockViews[(int)VolumeOrder.BattleSE]?.OnValueChanged(value);
    }

    public void SetUIVolume(float value) {
        sliderBlockViews[(int)VolumeOrder.UI]?.OnValueChanged(value);
    }

    public void SetFlashToggle(bool isOn) {
        flashToggle.isOn = isOn;
    }

    public void SetShakeToggle(bool isOn) {
        shakeToggle.isOn = isOn;
    }

    public void SetBattleAnimSpeedText(float value) {
        battleAnimSpeedText?.SetText(value + " 倍");
    }
    
    public void SetInitMapText(int value) {
        string home = (Map.GetMap(value)?.worldId ?? 0) switch
        {
            1 => "新世界",
            _ => "阿卡迪亚",
        };
        initMapText?.SetText(home);
    }

    public enum VolumeOrder
    {
        BGM = 0, BattleBGM = 1, BattleSE = 2, UI = 3,
    }
}

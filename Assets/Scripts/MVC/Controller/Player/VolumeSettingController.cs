using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSettingController : Module
{
    [SerializeField] private VolumeSettingModel volumeModel;
    [SerializeField] private VolumeSettingView volumeView;

    public override void Init()
    {
        base.Init();
        volumeModel.InitVolume();
        volumeView.SetSliderVolume(volumeModel.settingsData);
    }

    public void OnConfirmSettings() {
        volumeModel.OnConfirmSettings();
        volumeView.OnConfirmSettings();
    }

    public void OnBGMValueChanged(float value) {
        volumeModel.SetBGMVolume(value);
        volumeView.SetBGMVolume(volumeModel.BGMVolume);
    }

    public void OnBattleBGMValueChanged(float value) {
        volumeModel.SetBattleBGMVolume(value);
        volumeView.SetBattleBGMVolume(volumeModel.battleBGMVolume);
    }

    public void OnBattleSEValueChanged(float value) {
        volumeModel.SetBattleSEVolume(value);
        volumeView.SetBattleSEVolume(volumeModel.battleSEVolume);
    }

    public void OnUIValueChanged(float value) {
        volumeModel.SetUIVolume(value);
        volumeView.SetUIVolume(volumeModel.UIVolume);
    }

    public void OnBattleAnimSpeedChanged() {
        float speed = (volumeModel.battleAnimSpeed == 1) ? 2 : 1;
        volumeModel.SetBattleAnimSpeed(speed);
        volumeView.SetBattleAnimSpeedText(volumeModel.battleAnimSpeed);
    }
}

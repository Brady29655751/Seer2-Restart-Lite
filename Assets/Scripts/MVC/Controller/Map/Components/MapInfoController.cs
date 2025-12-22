using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfoController : UIModule
{
    [SerializeField] private MapInfoModel infoModel;
    [SerializeField] private MapInfoView infoView;

    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPositionOffset(new Vector2(10, -30));
    }

    public void SetMapInfoText(string text) {
        infoView.SetInfoText(text);
    }

    public void SetWeather(int weather) {
        infoModel.SetWeather(weather);
        infoView.SetWeather(weather);
    }

    public void ShowWeatherInfo() {
        infoPrompt.SetBuff(Buff.GetWeatherBuff(infoModel.weather));
    }

    public void SetDayNightSwitch(int dayNightSwitch) {
        infoModel.SetDayNightSwitch(dayNightSwitch);
        infoView.SetDayNightSwitch(infoModel.dayNightSwitch);
    }

    public void SwitchDayNight() {
        TeleportHandler.SwitchDayNight(infoModel.dayNightSwitch);
    }
}

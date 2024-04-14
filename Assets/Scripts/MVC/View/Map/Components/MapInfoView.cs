using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapInfoView : Module
{
    [SerializeField] private Text infoText;
    [SerializeField] private Image weatherBackground;
    [SerializeField] private IButton weatherButton;
    [SerializeField] private Image dayNightBackground;

    private void SetWeatherActive(bool active) {
        weatherBackground.gameObject.SetActive(active);
    }

    public void SetWeather(Weather weather) {
        bool isWeatherNull = (weather == Weather.无);
        weatherButton.image?.SetWeatherSprite(weather);
        SetWeatherActive(!isWeatherNull);
    }

    public void SetDayNightSwitch(int dayNightSwitch) {
        dayNightBackground?.gameObject.SetActive(dayNightSwitch != 0);
    }

    public void SetInfoText(string text) {
        if (infoText != null) {
            infoText.text = text;   
        }
    }

}

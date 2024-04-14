using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleWeatherView : BattleBaseView
{
    private bool isIconMode = false;
    private bool isWeatherNull = true;

    [SerializeField] private IButton button;
    [SerializeField] private Image weatherIcon;
    [SerializeField] private Text turnText;

    public void SwitchMode() {
        SetMode(!isIconMode);
    }

    public void SetMode(bool isIconMode) {
        this.isIconMode = (isIconMode && (!isWeatherNull));
        weatherIcon.gameObject.SetActive(this.isIconMode);
        turnText.gameObject.SetActive(!this.isIconMode);
    }

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;

        SetWeather(currentState.weather);
        SetTurn(currentState.turn);
    }

    public void SetWeather(Weather weather) {
        isWeatherNull = (weather == Weather.无);
        weatherIcon.SetWeatherSprite(weather);
        SetMode(!isWeatherNull);
    }

    public void SetTurn(int turn) {
        turnText.text = "回合\n" + turn.ToString();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleWeatherView : BattleBaseView
{
    private bool isIconMode = false;
    private bool isWeatherNull = true;
    private int weather = 0;

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

    public void SetTurn(int turn) {
        turnText.text = "回合\n" + turn.ToString();
    }

    public void SetWeather(int weather) {
        this.weather = weather;
        isWeatherNull = (weather == 0);
        weatherIcon.SetSprite(Buff.GetWeatherBuff(weather).icon);
        SetMode(!isWeatherNull);
    }

    public void SetWeatherInfoPromptActive(bool active) {
        if (!active) {
            SetInfoPromptActive(false);
            return;
        }

        if (!isIconMode)
            return;

        SetInfoPromptActive(true);
    }

    public void ShowWeatherDescription() {
        if (!isIconMode)
            return;

        infoPrompt.SetBuff(Buff.GetWeatherBuff(weather));
    }

}

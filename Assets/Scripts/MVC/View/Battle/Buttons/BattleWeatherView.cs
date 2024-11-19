using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleWeatherView : BattleBaseView
{
    [SerializeField] private Text turnText;

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;

        SetTurn(currentState.turn);
    }

    public void SetTurn(int turn) {
        turnText.text = "回合\n" + turn.ToString();
    }

}

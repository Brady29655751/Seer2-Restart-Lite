using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePlayerSwitchView : BattleBaseView
{
    [SerializeField] private GameObject switchBar;
    [SerializeField] private IButton switchButton;
    [SerializeField] private Text playerText;

    public void SetActive(bool active) {
        switchBar.SetActive(active);
    }

    public void SwitchPlayer(bool isMe) {
        playerText?.SetText((isMe ? "左方" : "右方") + "的选择");
    }
}

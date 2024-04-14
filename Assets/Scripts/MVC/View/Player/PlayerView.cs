using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : Module
{
    [SerializeField] private RectTransform playerRect;
    [SerializeField] private IButton playerButton;
    [SerializeField] private Sprite[] playerDirectionSprite = new Sprite[4];
    [SerializeField] private Text playerNameText;

    // 0: Front Left,   1: Back Left
    // 2: Front Right,  3: Back Right
    public Sprite GetPlayerDirectionSprite(Vector2 direction) {
        if (direction == Vector2.zero) {
            return null;
        }

        int idx = -1;
        if (direction.x <= 0) {
            idx = (direction.y <= 0) ? 0 : 1;
        } else {
            idx = (direction.y <= 0) ? 2 : 3;
        }
        return playerDirectionSprite[idx];
    }

    public void SetDirection(Vector2 direction) {
        Sprite directionSprite = GetPlayerDirectionSprite(direction);
        if (directionSprite != null) {
            playerButton.SetSprite(directionSprite); 
        }
    }

    public void SetPlayerPosition(Vector2 currentPos) {
        playerRect.anchoredPosition = currentPos;
    }

    public void SetPlayerName(string value) {
        playerNameText.text = value;
    }

    public PlayerInfoPanel OpenPlayerInfoPanel() {
        return Panel.OpenPanel<PlayerInfoPanel>();
    }
}

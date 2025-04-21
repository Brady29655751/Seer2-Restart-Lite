using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : Module
{
    [SerializeField] protected RectTransform playerRect;
    [SerializeField] protected IButton playerButton;
    [SerializeField] protected Sprite[] playerDirectionSprite = new Sprite[4];
    [SerializeField] protected Text playerNameText, achievementText;

    protected Vector2 lastDirection = Vector2.zero;

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

    public virtual void SetDirection(Vector2 direction) {
        Sprite directionSprite = GetPlayerDirectionSprite(direction);
        if (directionSprite != null) {
            playerButton.SetSprite(directionSprite); 
        }
        lastDirection = direction;
    }

    public void SetPlayerPosition(Vector2 currentPos) {
        playerRect.anchoredPosition = currentPos;
    }

    public void SetPlayerName(string value) {
        playerNameText.text = value;
    }

    public void SetPlayerAchievement(string value) {
        achievementText.text = value;
    }

    public PlayerInfoPanel OpenPlayerInfoPanel() {
        return Panel.OpenPanel<PlayerInfoPanel>();
    }
    
    public void SetPlayerSprite(Sprite sprite) {
        if (sprite != null) {
            playerButton.SetSprite(sprite); 
            // 使用 LINQ 优化赋值操作，将相同的 Sprite 赋值给数组中的每个元素
            playerDirectionSprite = Enumerable.Repeat(sprite, 4).ToArray();
        }
    }
}

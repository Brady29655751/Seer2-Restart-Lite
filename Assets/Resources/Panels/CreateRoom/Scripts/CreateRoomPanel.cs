using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : Panel
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private GameObject pvpNoteObject;
    
    // public override void Init() {
    //     pvpNoteObject?.SetActive(GameManager.instance.heliosMode);
    //     rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, GameManager.instance.heliosMode ? 80 : 0);
    // }
}

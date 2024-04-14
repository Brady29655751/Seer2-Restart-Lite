using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomView : Module
{
    [SerializeField] private Hintbox hintbox;
    [SerializeField] private Image optionBackground;

    public void SetPetCount(int count) {
        int posX = 200 + ((count == 1) ? 0 : 1) * 105;
        optionBackground.rectTransform.anchoredPosition = new Vector2(posX, 0);
    }

    public void CreateRoom() {
        NetworkManager.instance.onCreateOrJoinFailedEvent += OnCreateRoomFailed;

        hintbox.SetActive(true);
        hintbox.SetTitle("提示");
        hintbox.SetContent("正在创建房间，请稍候", 18, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }

    private void OnCreateRoomFailed(short code, string message) {
        hintbox.SetActive(false);

        NetworkManager.instance.onCreateOrJoinFailedEvent -= OnCreateRoomFailed;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class JoinRoomView : Module
{
    [SerializeField] private Hintbox hintbox;

    public void JoinRoom() {
        NetworkManager.instance.onCreateOrJoinFailedEvent += OnJoinRoomFailed;

        hintbox.SetTitle("提示");
        hintbox.SetContent("正在加入房间，请稍候", 18, FontOption.Arial);
        hintbox.SetOptionNum(1);
        hintbox.SetActive(true);
    }

    private void OnJoinRoomFailed(short code, string message) {
        hintbox.SetActive(false);

        NetworkManager.instance.onCreateOrJoinFailedEvent -= OnJoinRoomFailed;
    }
}

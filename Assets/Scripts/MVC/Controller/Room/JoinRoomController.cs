using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinRoomController : Module
{
    [SerializeField] private JoinRoomModel joinModel;
    [SerializeField] private JoinRoomView joinView;

    public void JoinRoom() {
        joinView.JoinRoom();
        joinModel.JoinRoom();
    }

}

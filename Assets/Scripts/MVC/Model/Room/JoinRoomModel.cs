using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinRoomModel : Module
{
    [SerializeField] private IInputField inputField;

    public string roomNum => inputField.inputString;

    public void JoinRoom() {
        NetworkData networkData = new NetworkData() {
            networkAction = NetworkAction.Join,
            roomName = roomNum,
        };
        NetworkManager.instance.StartNetworkAction(networkData);
    }
}

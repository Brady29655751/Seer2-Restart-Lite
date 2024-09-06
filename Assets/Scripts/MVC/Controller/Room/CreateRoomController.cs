using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomController : Module
{
    [SerializeField] private CreateRoomModel createModel;
    [SerializeField] private CreateRoomView createView;

    public void SetPetCount(int count) {
        createModel.SetPetCount(count);
        createView.SetPetCount(count);
    }

    public void SetTurnTime(int time) {
        createModel.SetTurnTime(time);
        createView.SetTurnTime(time);
    }

    public void CreateRoom() {
        createView.CreateRoom();
        createModel.CreateRoom();
    }
}
